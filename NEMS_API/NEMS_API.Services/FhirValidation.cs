using Hl7.Fhir.Model;
using Hl7.Fhir.Validation;
using Microsoft.Extensions.Options;
using NEMS_API.Core.Factories;
using NEMS_API.Core.Interfaces.Helpers;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Core.Resources;
using NEMS_API.Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NEMS_API.Services
{
    public class FhirValidation : IFhirValidation
    {
        private readonly IValidationHelper _validationHelper;
        private readonly NemsApiSettings _nemsApiSettings;

        public FhirValidation(IOptions<NemsApiSettings> nemsApiSettings, IValidationHelper validationHelper)
        {
            _validationHelper = validationHelper;
            _nemsApiSettings = nemsApiSettings.Value;
        }

        public OperationOutcome ValidProfile<T>(T resource, string customProfile) where T : Resource
        {
            var customProfiles = new List<string>();

            if (!string.IsNullOrEmpty(customProfile))
            {
                customProfiles.Add(customProfile);
            }

            var result = _validationHelper.Validator.Validate(resource, customProfiles.ToArray());

            return result;
        }

        public OperationOutcome ValidSubscription(Subscription subscription)
        {

            if(subscription.Meta != null)
            {
                if(subscription.Meta.LastUpdated != null)
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

            //if (subscription.Status != Subscription.SubscriptionStatus.Requested)
            //{
            //    return OperationOutcomeFactory.CreateInvalidResource("Subscription.Status", "Status must be of the value Requested.");
            //}

            //Channel should not be null at this point based on profile validation
            if (subscription.Channel != null)
            {
                if (subscription.Channel.Type != Subscription.SubscriptionChannelType.Message)
                {
                    return OperationOutcomeFactory.CreateInvalidResource("Subscription.Channel.Type", "Channel.Type must be of the value Message.");
                }

                //TODO: Should we create a mapping between ficticious mailbox ids and ods codes?
                if (string.IsNullOrEmpty(subscription.Channel.Endpoint) || !FhirUri.IsValidValue(subscription.Channel.Endpoint))
                {
                    return OperationOutcomeFactory.CreateInvalidResource("Subscription.Channel.Endpoint", "Channel.Endpoint must be supplied.");
                }
            }

            //Contact should not be null or zero at this point based on profile validation
            if (subscription.Contact != null && subscription.Contact.Count > 0)
            {
                if(subscription.Contact.First().System != ContactPoint.ContactPointSystem.Url)
                {
                    return OperationOutcomeFactory.CreateInvalidResource("Subscription.Contact[0].System", "Contact[0].System must be of the value url.");
                }

                if (subscription.Contact.First().Use != ContactPoint.ContactPointUse.Work)
                {
                    return OperationOutcomeFactory.CreateInvalidResource("Subscription.Contact[0].Use", "Contact[0].Use must be of the value work.");
                }
            }

            var criteriaOutcome = ValidateSubscriptionCriteria(subscription.Criteria);

            if (!criteriaOutcome.Success)
            {
                return criteriaOutcome;
            }


            return OperationOutcomeFactory.CreateOk();
        }

        public OperationOutcome ValidateSubscriptionCriteria(string criteria)
        {

            if (_nemsApiSettings.SkipSubscriptionCriteria)
            {
                return OperationOutcomeFactory.CreateOk();
            }

            var ruleSet = ParseSubscriptionCriteriaRules(_nemsApiSettings.SubscriptionCriteriaRules);

            if (ruleSet == null || ruleSet.Rules.Count == 0)
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

            if (!string.IsNullOrEmpty(ruleSet.StartsWith) && ruleSet.StartsWith != subscriptionCriteria.Base)
            {
                (criteriaSuccess, criteriaMessage) = (false, $"Criteria resource type must be of type {ruleSet.StartsWith}.");
            }
            else if(!subscriptionCriteria.Parameters.Any())
            {
                //Assuming crieria will always have at least two components including the base
                (criteriaSuccess, criteriaMessage) = (false, $"Criteria does not have any components.");
            }
            else
            {
                //validate other components
                //TODO: pull search parameter rules from profile
                //TODO: upgrade app settings to include api setting or external valueset list etc

                foreach(var rule in ruleSet.Rules)
                {
                    if (rule.Key.ToLowerInvariant() == "servicetype" && rule.Min > 0 && _nemsApiSettings.ServiceTypeCodes.Count > 0)
                    {
                        var serviceType = subscriptionCriteria.Parameters.FirstOrDefault(x => x.Key.ToLowerInvariant() == "servicetype");
                        if (string.IsNullOrEmpty(serviceType.Key) || !_nemsApiSettings.ServiceTypeCodes.Contains(serviceType.Value))
                        {
                            (criteriaSuccess, criteriaMessage) = (false, $"Criteria servicetype contains an invalid code.");
                            break;
                        }
                    }
                    else
                    {
                        //Find all criteria params that match rule key
                        var param = subscriptionCriteria.Parameters.Where(x => x.Key.ToLowerInvariant() == rule.Key.ToLowerInvariant());

                        //Must contain exactly x of param type
                        if (rule.Min == rule.Max && param.Count() != rule.Max)
                        {
                            (criteriaSuccess, criteriaMessage) = (false, $"Criteria must contain exactly {rule.Min} {rule.Key} parameters.");
                            break;
                        }

                        //Must have a least one of param type
                        if (rule.Min > 0 && param.Count() < 1)
                        {
                            (criteriaSuccess, criteriaMessage) = (false, $"Criteria must contain at least {rule.Min} {rule.Key} parameter.");
                            break;
                        }

                        //Check that total of param type does not exceed max
                        if (param.Count() > rule.Max)
                        {
                            (criteriaSuccess, criteriaMessage) = (false, $"Criteria must contain a maximum of {rule.Max} {rule.Key} parameters.");
                            break;
                        }

                        //If we require at least one of param type make sure its value is not null
                        if (rule.Min > 0 && !param.All(x => !string.IsNullOrEmpty(x.Value)))
                        {
                            (criteriaSuccess, criteriaMessage) = (false, $"Criteria parameters {rule.Key} must contain values.");
                            break;
                        }


                        //If param type should be exact value in rule check param values match this
                        if (rule.ExactValue && !param.All(x => !string.IsNullOrEmpty(x.Value) && x.Value == rule.Value))
                        {
                            (criteriaSuccess, criteriaMessage) = (false, $"Criteria parameter value for {rule.Key} must be {rule.Value}.");
                            break;
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

        public SubscriptionCriteriaRuleSet ParseSubscriptionCriteriaRules(List<string> rules)
        {
            if (rules == null || rules.Count == 0)
            {
                return null;
            }

            var criteria = new SubscriptionCriteriaRuleSet
            {
                StartsWith = rules.FirstOrDefault(x => x.StartsWith("^"))?.Replace("^", "")
            };

            var ruleSet = rules.Where(x => !x.StartsWith("^")).ToList();

            ruleSet.ForEach(rule => {

                var ruleKV = string.IsNullOrWhiteSpace(rule) ? new List<string>() : rule.Split("=").ToList();

                if(ruleKV.Count == 2 && !string.IsNullOrWhiteSpace(ruleKV.ElementAt(1)))
                {
                    var ruleKey = ruleKV.ElementAt(0);
                    var ruleValue = ruleKV.ElementAt(1);

                    var isZeroOrMore = ruleValue.StartsWith("+");
                    var isAtLeastOneOrMore = ruleValue.StartsWith("#") && ruleValue.EndsWith("+");
                    var hasMax = ruleValue.EndsWith("#");
                    var isExactValue = !(ruleValue.Contains("-") || ruleValue.Contains("+") || ruleValue.Contains("#"));

                    var totalParam = ruleValue.Count(x => x.Equals('#'));

                    var criteriaRule = new SubscriptionCriteriaRule
                    {
                        Key = ruleKey,
                        Value = isExactValue ? ruleValue : "*",
                        ExactValue = isExactValue,
                        Min = isZeroOrMore ? 0 : totalParam,
                        Max = hasMax ? totalParam : 100
                    };

                    criteria.Rules.Add(criteriaRule);
                }

            });

            return criteria;
        }

    }
}
