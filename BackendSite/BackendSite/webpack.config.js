var path = require('path');
var webpack = require('webpack');
//const UglifyJSPlugin = require('uglifyjs-webpack-plugin');

module.exports = {
    entry: {
        "js/component/Withdrawal/WithdrawalList.min": [
            '../BackendSite/wwwroot/js/component/Withdrawal/WithdrawalList.jsx'
        ],
        //"js/component/Deposit/DepositList.min": [
        //    '../BackendSite/wwwroot/js/component/Deposit/DepositList.jsx'
        //],
        // "js/component/Finance/DepositSubmission.min": [
        //     '../BackendSite/wwwroot/js/component/Finance/DepositSubmission.jsx'
        // ], 
        "js/component/Payment/BankInfoList.min": [
            '../BackendSite/wwwroot/js/component/Payment/BankInfoList.jsx'
        ],
        // "js/component/Finance/PaymentAdjustment.min": [
        //     '../BackendSite/wwwroot/js/component/Finance/PaymentAdjustment.jsx'
        // ],
        // "js/component/CustManage/CustInfo.min": [
        //    '../BackendSite/wwwroot/js/component/CustManage/CustInfo.jsx'
        // ],
        // "js/component/CustManage/CustInfoList.min": [
        //     '../BackendSite/wwwroot/js/component/CustManage/CustInfoList.jsx'
        // ],
        // "js/component/Marketing/PointLevelList.min": [
        //     '../BackendSite/wwwroot/js/component/Marketing/PointLevelList.jsx'
        // ],
    },
    output: {
        path: path.resolve(__dirname, '../BackendSite/wwwroot/'),
        filename: "[name].js",
        chunkFilename: "[name].chunk.js?v=" + new Date().getTime(),
        publicPath: ''
    },
    module: {
        rules: [
            {
                test: /\.(js|jsx)$/,
                loader: 'babel-loader',
                exclude: /node_modules/,
                query: {
                    "presets": [
                        "@babel/preset-env",
                        "@babel/preset-react"
                    ],
                    plugins: [
                        [
                            "@babel/plugin-proposal-class-properties"
                        ],
                        ["@babel/plugin-transform-runtime"]
                    ]
                }
            }
        ]
    }
};