const path = require('path');
//const webpack = require('webpack');
//const bundleAnalyzer = require("webpack-bundle-analyzer").BundleAnalyzerPlugin;

module.exports = {
    mode:'production',
    optimization: {
        mangleWasmImports: true,
        nodeEnv: 'production'
    },
    externals:{
        'react': 'React',
        'react-dom': 'ReactDOM',
        'moment':'moment',
        'jquery':'$',
        'axios':'axios'
    },
    entry: {
        "Administration/UserAuthoritySetting.min":[
            // '@babel/polyfill',
            './src/pages/Administration/UserAuthoritySetting/index.js'
        ],
        "Administration/GroupAuthoritySetting.min":[
            // '@babel/polyfill',
            './src/pages/Administration/GroupAuthoritySetting/index.js'
        ],
        "Report/LicenseeWinLost.min":[
            // '@babel/polyfill',
            './src/pages/Report/LicenseeWinLost/index.js'
        ],
        "Finance/PaymentAdjustmentReport.min":[
            // '@babel/polyfill',
            './src/pages/Finance/PaymentAdjustmentReport/index.js'
        ],
        "Marketing/BonusInfoReport.min":[
            // '@babel/polyfill',
            './src/pages/Marketing/BonusInfoReport/index.js'
        ],
        "Finance/PaymentManagement.min":[
            // '@babel/polyfill',
            './src/pages/Finance/PaymentManagement/index.js'
        ],
        "CSD/Announcement.min":[
            './src/pages/CSD/Announcement/index.js'
        ],
        "Finance/WithdrawalLimitAdjust.min":[
            './src/pages/Finance/WithdrawalLimitAdjust/index.js'
        ],
        "CSD/PersonalMsgBatch.min":[
            './src/pages/CSD/PersonalMsgBatch/index.js'
        ]
    },
    output:{
        path:  path.resolve(__dirname,'../BackendSite/wwwroot/js/component/'),
        filename:"[name].js"
    },
    resolve: {
        extensions: ['.js', '.jsx']
    },
    module:{
        rules:[
            {
                test: /\.(js|jsx)$/,
                exclude: /(node_modules)/,
                use: {
                    loader: 'babel-loader',
                    options: {
                        presets: [
                            ['@babel/preset-env',{
                                    "corejs": "3",
                                    useBuiltIns: 'entry'
                                },
                            ],
                            '@babel/preset-react',{
                                'plugins': ['@babel/plugin-proposal-class-properties']
                            }
                        ]
                    }
                }
            },
            {
                test: /\.css$/,
                // exclude: /(node_modules)/,
                use: ['style-loader', 'css-loader'],
            }
        ]
    }
}