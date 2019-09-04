const path = require('path');
const { AureliaPlugin } = require('aurelia-webpack-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const { ProvidePlugin } = require('webpack');
const { TsConfigPathsPlugin, CheckerPlugin } = require('awesome-typescript-loader');

const outDir = path.resolve(__dirname, 'wwwroot');
const srcDir = path.resolve(__dirname, 'AppSrc');
const staticContentDir = path.resolve(__dirname, 'static');
const nodeModulesDir = path.resolve(__dirname, 'node_modules');
const baseUrl = '/';
const title = "NEMS Demonstrator Apps"

const ensureArray = (config) => config && (Array.isArray(config) ? config : [config]) || []
const when = (condition, config, negativeConfig) =>
    condition ? ensureArray(config) : ensureArray(negativeConfig)

//const cssRules = [
//    { loader: 'css-loader' },
//    {
//        loader: 'postcss-loader',
//        options: { plugins: () => [require('autoprefixer')({ browsers: ['last 2 versions'] })] }
//    }
//]
  
module.exports = ({ production, server, extractCss, coverage } = {}) => ({
      resolve: {
        extensions: ['.ts', '.js'],
        modules: ['AppSrc', 'node_modules']
      },
      entry: ['aurelia-bootstrapper'],
      output: {
          path: outDir,
          publicPath: baseUrl,
          filename: production ? '[name].[chunkhash].bundle.js' : '[name].[hash].bundle.js',
          sourceMapFilename: production ? '[name].[chunkhash].bundle.map' : '[name].[hash].bundle.map',
          chunkFilename: production ? '[name].[chunkhash].chunk.js' : '[name].[hash].chunk.js',
      },
      devServer: {
          contentBase: outDir,
          historyApiFallback: true,
    },
    optimization: {
        splitChunks: {
            cacheGroups: {
                styles: {
                    name: 'styles',
                    test: /\.css$/,
                    chunks: 'all',
                    enforce: true,
                },
            },
        },
    },
    module: {
        rules: [
            {
                test: /\.(sa|sc|c)ss$/,
                use: [
                    extractCss ? MiniCssExtractPlugin.loader : 'style-loader',
                    'css-loader',
                    'sass-loader',
                ],
            },
            // CSS required in JS/TS files should use the style-loader that auto-injects it into the website
            // only when the issuer is a .js/.ts file, so the loaders are not applied inside html templates
            //{
            //    test: /\.css$/i,
            //    issuer: [{ not: [{ test: /\.html$/i }] }],
            //    use: extractCss ? [MiniCssExtractPlugin.loader, 'css-loader'] : ['style-loader', ...cssRules],
            //},
            //{
            //    test: /\.css$/i,
            //    issuer: [{ test: /\.html$/i }],
            //    // CSS required in templates cannot be extracted safely
            //    // because Aurelia would try to require it again in runtime
            //    use: cssRules,
            //},
            { test: /\.html$/i, loader: 'html-loader' },
            { test: /\.ts$/i, loader: 'awesome-typescript-loader', exclude: [nodeModulesDir, staticContentDir ] },
            { test: /\.json$/i, loader: 'json-loader' },
            // use Bluebird as the global Promise implementation:
            { test: /[\/\\]node_modules[\/\\]bluebird[\/\\].+\.js$/, loader: 'expose-loader?Promise' },
            // exposes jQuery globally as $ and as jQuery:
            { test: require.resolve('jquery'), loader: 'expose-loader?$!expose-loader?jQuery' },
            // embed small images and fonts as Data Urls and larger ones as files:
            { test: /\.(png|gif|jpg|cur)$/i, loader: 'url-loader', options: { limit: 8192 } },
            { test: /\.woff2(\?v=[0-9]\.[0-9]\.[0-9])?$/i, loader: 'url-loader', options: { limit: 10000, mimetype: 'application/font-woff2' } },
            { test: /\.woff(\?v=[0-9]\.[0-9]\.[0-9])?$/i, loader: 'url-loader', options: { limit: 10000, mimetype: 'application/font-woff' } },
            // load these fonts normally, as files:
            { test: /\.(ttf|eot|svg|otf)(\?v=[0-9]\.[0-9]\.[0-9])?$/i, loader: 'file-loader' },
            ...when(coverage, {
                test: /\.[jt]s$/i, loader: 'istanbul-instrumenter-loader',
                include: srcDir, exclude: [/\.{spec,test}\.[jt]s$/i],
                enforce: 'post', options: { esModules: true },
            })
        ]
    },
      plugins: [
          new AureliaPlugin(),
        new ProvidePlugin({
            'Promise': 'bluebird',
            '$': 'jquery',
            'jQuery': 'jquery',
            'window.jQuery': 'jquery',
        }),
        new TsConfigPathsPlugin(),
        new CheckerPlugin(),
        new HtmlWebpackPlugin({
            template: 'index.ejs',
            minify: production ? {
                removeComments: true,
                collapseWhitespace: true
            } : undefined,
            metadata: {
                // available in index.ejs //
                title, server, baseUrl
            },
        }),
        new CopyWebpackPlugin([
            { from: 'static/**/*', to: '' }
          ]),
        new CopyWebpackPlugin([
            { from: 'Data/examples/**/*', to: '' }
        ]),
        ...when(extractCss, new MiniCssExtractPlugin({
              filename: production ? '[name].[hash].css' : '[name].css'
        }))
    ]
});

  