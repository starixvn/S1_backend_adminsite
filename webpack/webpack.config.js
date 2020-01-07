var path = require('path');
var webpack = require('webpack');
//const UglifyJSPlugin = require('uglifyjs-webpack-plugin');

module.exports = {
  entry:{
		"js/component/Withdrawal/WithdrawalList.min":[
			'../BackendSite/BackendSite/wwwroot/js/component/Withdrawal/WithdrawalList.jsx'
    ],"js/component/Deposit/DepositList.min":[
			'../BackendSite/BackendSite/wwwroot/js/component/Deposit/DepositList.jsx'
    ]
  },
  output: {
    path:  path.resolve(__dirname,'../BackendSite/BackendSite/wwwroot/'),
    filename: "[name].js",
	chunkFilename: "[name].chunk.js?v=" + new Date().getTime(),
	publicPath: ''
  },
  module: {
    rules : [
      {
        test: /\.(js|jsx)$/,
        loader: 'babel-loader',
        query: {
            "presets": [
                "@babel/preset-env",
                "@babel/preset-react"
            ],
            plugins: [
                [
                  "@babel/plugin-proposal-class-properties"
                ]
            ]
        }
      }
    ]
  }
};
