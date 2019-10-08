using Hl7.Fhir.Model;
using Microsoft.Extensions.Options;
using NEMS_API.Core.Exceptions;
using NEMS_API.Core.Factories;
using NEMS_API.Core.Interfaces.Helpers;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.Core;
using NEMS_API.Models.FhirResources;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace NEMS_API.Services
{
    public class FhirValidation : IFhirValidation
    {
        private readonly IValidationHelper _validationHelper;
        private readonly ISdsService _sdsService;
        private readonly NemsApiSettings _nemsApiSettings;

        public FhirValidation(IOptions<NemsApiSettings> nemsApiSettings, IValidationHelper validationHelper, ISdsService sdsService)
        {
            _validationHelper = validationHelper;
            _sdsService = sdsService;
            _nemsApiSettings = nemsApiSettings.Value;
        }

        public OperationOutcome ValidProfile<T>(T resource, string customProfile) where T : Resource
        {
            var result = _validationHelper.ValidateResource(resource, customProfile);

            return result;
        }

        public OperationOutcome ValidSubscription(NemsSubscription subscription)
        {
            //#META
            if (subscription.Meta != null)
            {
                if(subscription.Meta.LastUpdated.HasValue)
                {
                    return OperationOutcomeFactory.CreateInvalidResource("Subscription.Meta.LastUpdated", "Meta.LastUpdated must not be provided by the subscriber.");
                }

                if (!string.IsNullOrEmpty(subscription.Meta.VersionId))
                {
                    return OperationOutcomeFactory.CreateInvalidResource("Subscription.Meta.VersionId", "Meta.VersionId must not be provided by the subscriber.");
                }
            }

            if (!string.IsNullOrEmpty(subscription.Id))
            {
                return OperationOutcomeFactory.CreateInvalidResource("Subscription.Id", "Id must not be provided by the subscriber.");
            }

            //#STATUS
            //Skip message review and go straight to "active"
            if (subscription.Status != Subscription.SubscriptionStatus.Requested)
            {
                return OperationOutcomeFactory.CreateInvalidResource("Subscription.Status", "Status must be of the value Requested.");
            }

            subscription.Status = Subscription.SubscriptionStatus.Active;

            //#CHANNEL
            //Channel should not be null at this point based on profile validation
            if (subscription.Channel.Type != Subscription.SubscriptionChannelType.Message)
            {
                return OperationOutcomeFactory.CreateInvalidResource("Subscription.Channel.Type", "Channel.Type must be of the value Message.");
            }

            if (string.IsNullOrEmpty(subscription.Channel.Endpoint) || !FhirUri.IsValidValue(subscription.Channel.Endpoint))
            {
                return OperationOutcomeFactory.CreateInvalidResource("Subscription.Channel.Endpoint", "Channel.Endpoint must be supplied.");
            }

            //#CONACT
            //Contact should not be null or zero at this point based on profile validation
            if(subscription.Contact.First().System != ContactPoint.ContactPointSystem.Url)
            {
                return OperationOutcomeFactory.CreateInvalidResource("Subscription.Contact[0].System", "Contact[0].System must be of the value url.");
            }

            if (subscription.Contact.First().Use != ContactPoint.ContactPointUse.Work)
            {
                return OperationOutcomeFactory.CreateInvalidResource("Subscription.Contact[0].Use", "Contact[0].Use must be of the value work.");
            }

            var odsRegex = new Regex($"^{_nemsApiSettings.ResourceUrl.OrganisationReferenceUrl}(.+)$");
            var odsMatch = odsRegex.Match(subscription.Contact.First().Value);

            if (!odsMatch.Success)
            {
                return OperationOutcomeFactory.CreateInvalidResource("Subscription.Contact[0].Value", $"Contact[0].Use must be of the format {_nemsApiSettings.ResourceUrl.OrganisationReferenceUrl}[Org_ODS_Code].");
            }

            //TODO: check subscription.Channel.Endpoint (MESH mailbox id) maps to subscription.Contact[0].Value ODS code
            var odsCode = odsMatch.Groups.ElementAt(1).Value;

            if (subscription.RequesterOdsCode != odsCode)
            {
                return OperationOutcomeFactory.CreateAccessDenied();
            }

            var client = _sdsService.GetFor(subscription.RequesterAsid);

            if (client == null)
            {
                throw new HttpFhirException("Invalid/Missing Header", OperationOutcomeFactory.CreateInvalidHeader("fromASID", null), HttpStatusCode.BadRequest);
            }

            if(subscription.Channel.Endpoint != client.MeshMailboxId)
            {
                throw new HttpFhirException("Subscriber MESH Mailbox is not associated with the supplied ODS code", OperationOutcomeFactory.CreateAccessDenied(), HttpStatusCode.Forbidden);
            }


            //#CRITERIA
            var criteriaOutcome = ValidateSubscriptionCriteria(subscription.CriteriaDecoded);

            if (!criteriaOutcome.Success)
            {
                return criteriaOutcome;
            }


            return OperationOutcomeFactory.CreateOk();
        }

        public OperationOutcome ValidateSubscriptionCriteria(string criteria)
        {

            if (_nemsApiSettings.ValidationOptions.SkipSubscriptionCriteria)
            {
                return OperationOutcomeFactory.CreateOk();
            }

            var ruleSet = _nemsApiSettings.SubscriptionCriteriaRules;

            if (ruleSet == null || ruleSet.Count == 0)
            {
                return OperationOutcomeFactory.CreateInternalError("SubscriptionCriteriaRules app setting is missing or does not contain any valid rules.");
            }

            var (criteriaSuccess, criteriaMessage) = (true, "");

            //criteria should never be null based on profile validation
            if (string.IsNullOrWhiteSpace(criteria))
            {
                (criteriaSuccess, criteriaMessage) = (false, "Criteria must be provided.");
            }

            var subscriptionCriteria = new SubscriptionCriteria(criteria);

            if(!subscriptionCriteria.Parameters.Any()) // || subscriptionCriteria.Parameters.Any(x => !ruleSet.Select(y => y.Parameter).ToList().Contains(x.Key)))
            {
                //Assuming crieria will always have at least two components including the base
                (criteriaSuccess, criteriaMessage) = (false, $"Criteria does not have any components.");
            }
            else
            {
                //validate other components
                //TODO: pull search parameter rules from profile
                //TODO: upgrade app settings to include api setting or external valueset list etc

                foreach(var rule in ruleSet)
                {
                    //Find all criteria params that match rule key
                    var ruleParams = subscriptionCriteria.Parameters.Where(x => x.Key == rule.Parameter);

                    if (rule.Ignore)
                    {
                        continue;
                    }

                    if(rule.Min == 0 && ruleParams.Count() == 0)
                    {
                        continue;
                    }


                    //Only expect a type of string to have one parameter
                    if (rule.ValueType == "single")
                    {
                        if (ruleParams.First().Value != rule.Values)
                        {
                            var message = rule.IsPrefix ? $"Criteria does not start with {rule.Values}" : $"Criteria parameter {rule.Parameter} is invalid.";

                            (criteriaSuccess, criteriaMessage) = (false, message);
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    //Must contain exactly x of param type
                    if (rule.Min == rule.Max && ruleParams.Count() != rule.Max)
                    {
                        (criteriaSuccess, criteriaMessage) = (false, $"Criteria must contain exactly {rule.Min} {rule.Parameter} parameters.");
                        break;
                    }

                    //Must have a least one of param type
                    if (rule.Min > 0 && ruleParams.Count() < 1)
                    {
                        (criteriaSuccess, criteriaMessage) = (false, $"Criteria must contain at least {rule.Min} {rule.Parameter} parameter.");
                        break;
                    }

                    //Check that total of param type does not exceed max
                    //If max is less than min then max is unlimited
                    if (rule.Max > rule.Min && ruleParams.Count() > rule.Max)
                    {
                        (criteriaSuccess, criteriaMessage) = (false, $"Criteria must contain a maximum of {rule.Max} {rule.Parameter} parameters.");
                        break;
                    }

                    //Make sure all provided params are not null
                    if (!ruleParams.All(x => !string.IsNullOrEmpty(x.Value)))
                    {
                        (criteriaSuccess, criteriaMessage) = (false, $"Criteria parameters {rule.Parameter} must contain values.");
                        break;
                    }


                    if(rule.ValueType == "regExp")
                    {
                        var reg = new Regex(@"" + rule.Values);

                        if(ruleParams.Any(x => !reg.Match(x.Value).Success))
                        {
                            (criteriaSuccess, criteriaMessage) = (false, $"Criteria parameters {rule.Parameter} is invalid.");
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (rule.ValueType == "codeSystem")
                    {
                        var codeSystem = _validationHelper.GetCodeSystem(rule.Values);

                        var codes = codeSystem?.Concept.Select(x => x.Code.Replace("\u200B","")).ToList() ?? new List<string>();

                                                //TODO: Advanced search of codesystem
                        if (codes.Count > 0 && !ruleParams.All(x => codes.Contains(x.Value)))
                        {
                            (criteriaSuccess, criteriaMessage) = (false, $"Criteria parameters {rule.Parameter} is invalid.");
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (rule.ValueType == "searchParameter")
                    {
                        var paramOptions = _validationHelper.GetSearchParameter(rule.Values);

                        //if (!ruleParams.All(x => x.Key.ToUpperInvariant() != paramOptions.Code.ToUpperInvariant()))
                        //{
                        //    (criteriaSuccess, criteriaMessage) = (false, $"Criteria parameters {rule.Parameter} contains invalid SearchParameter definition.");
                        //    break;
                        //}

                        //Currently just parsing patient.age
                        //TODO: Advanced handling of searchparameters
                        var prefixMatcher = paramOptions.Comparator.Any(x => x.HasValue) ? "(" + string.Join("|", paramOptions.Comparator.Where(x => x.HasValue).Select(x => x.Value.ToString().ToLowerInvariant()).ToList()) + ")?" : "";
                        var valueTypeMatcher = paramOptions.Type.HasValue && paramOptions.Type.Value == SearchParamType.Number ? @"(\d+)" : "(.+)"; //else default to string
                        var prefixReg = new Regex(@"^" + prefixMatcher + valueTypeMatcher + @"$");

                        if (ruleParams.Any(x => !prefixReg.Match(x.Value).Success))
                        {
                            (criteriaSuccess, criteriaMessage) = (false, $"Criteria parameters {rule.Parameter} is invalid.");
                            break;
                        }
                        else
                        {
                            continue;
                        }

                    }

                }
            }

            if (!criteriaSuccess)
            {
                return OperationOutcomeFactory.CreateInvalidResource("Subscription.Criteria", criteriaMessage);
            }

            return OperationOutcomeFactory.CreateOk();
        }
              

    }
}
