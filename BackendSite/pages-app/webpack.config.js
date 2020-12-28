const path = require("path");
//const webpack = require('webpack');
//const bundleAnalyzer = require("webpack-bundle-analyzer").BundleAnalyzerPlugin;

module.exports = {
	mode: "production",
	optimization: {
		mangleWasmImports: true,
		nodeEnv: "production"
	},
	externals: {
		react: "React",
		"react-dom": "ReactDOM",
		moment: "moment",
		jquery: "$",
		axios: "axios"
	},
	entry: {
		// "Administration/UserAuthoritySetting.min": [
		// 	// '@babel/polyfill',
		// 	"./src/pages/Administration/UserAuthoritySetting/index.js"
		// ],
		// "Administration/GroupAuthoritySetting.min": [
		// 	// '@babel/polyfill',
		// 	"./src/pages/Administration/GroupAuthoritySetting/index.js"
		// ],
		// "Report/LicenseeWinLost.min": [
		// 	// '@babel/polyfill',
		// 	"./src/pages/Report/LicenseeWinLost/index.js"
		// ],
		// "Finance/PaymentAdjustmentReport.min": [
		// 	// '@babel/polyfill',
		// 	"./src/pages/Finance/PaymentAdjustmentReport/index.js"
		// ],
		// "Marketing/BonusInfoReport.min": [
		// 	// '@babel/polyfill',
		// 	"./src/pages/Marketing/BonusInfoReport/index.js"
		// ],
		// "Finance/PaymentManagement.min": [
		// 	// '@babel/polyfill',
		// 	"./src/pages/Finance/PaymentManagement/index.js"
		// ],
		// "CSD/Announcement.min": ["./src/pages/CSD/Announcement/index.js"],
		// "Finance/WithdrawalLimitAdjust.min": [
		// 	"./src/pages/Finance/WithdrawalLimitAdjust/index.js"
		// ],
		//"CSD/PersonalMsgBatch.min": [
		//	"./src/pages/CSD/PersonalMsgBatch/index.js"
		//],
		// "Marketing/PointLevelList.min": [
		// 	"./src/pages/Marketing/PointLevelManagement/index.js"
		// ],
		// "Marketing/CashBackBonus.min": [
		// 	"./src/pages/Marketing/CashBackBonus/index.jsx"
		// ],
		// "Finance/DepositLimitManagement.min": [
		// 	"./src/pages/Finance/DepositLimitManagement/index.js"
		// ],
		// "Finance/WithdrawalLimitManagement.min": [
		// 	"./src/pages/Finance/WithdrawalLimitManagement/index.js"
		// ],
		// "CSD/WithdrawalKYCControl.min": [
		// 	"./src/pages/CSD/WithdrawalKYCControl/index.js"
		// ],
		// "Home/DashBoard.min": ["./src/pages/Home/DashBoard/index.js"],
		// "Finance/BalanceAdjustmentBatch.min": [
		// 	"./src/pages/Finance/BalanceAdjustmentBatch/index.js"
		// ],
		// "CSD/ChangeVipLevelBatch.min": [
		// 	"./src/pages/CSD/ChangeVipLevelBatch/index.js"
		// ],
		// "Marketing/BonusManage.min": [
		// 	"@babel/polyfill",
		// 	"./src/pages/Marketing/BonusManage/index.jsx"
		// ],
		// "Marketing/PendingBonus.min": [
		// 	"./src/pages/Marketing/PendingBonus/index.jsx"
		// ],
		// "Finance/ThirdPartyAdjustment.min": [
		// 	"./src/pages/Finance/ThirdPartyAdjustment/index.js"
		// ],
		// "Marketing/NewsManage.min": [
		// 	"./src/pages/Marketing/NewsManage/index.js"
		// ],
		// "Marketing/RebateLogReport.min": [
		// 	// '@babel/polyfill',
		// 	"./src/pages/Marketing/RebateLogReport/index.js"
		// ],
		// "Marketing/RebateInfoReport.min": [
		// 	// '@babel/polyfill',
		// 	"./src/pages/Marketing/RebateInfoReport/index.js"
		// ],
		// "Report/MainFinancialReport.min": [
		// 	"./src/pages/Report/MainFinancialReport/index.js"
		// ],
		// "Marketing/BannerManage.min": [
		// 	'@babel/polyfill',
		// 	"./src/pages/Marketing/BannerManage/index.jsx"
		// ],
		// "Report/MainFinancialReport.min": [
		// 	"./src/pages/Report/MainFinancialReport/index.js"
		// ],
		"Report/DepositReport.min": [
			"./src/pages/Report/DepositReport/index.js"
		],
		// "Report/StatisticalReport.min": [
		// 	"./src/pages/Report/StatisticalReport/index.js"
		// ],
		// "Report/UserOnlineReport.min": [
		// 	"./src/pages/Report/UserOnlineReport/index.js"
		// ],
		// "Marketing/BonusImport.min": [
		// 	"@babel/polyfill",
		// 	"./src/pages/Marketing/BonusImport/index.js"
		// ],
	},
	output: {
		path: path.resolve(__dirname, "../BackendSite/wwwroot/js/component/"),
		filename: "[name].js"
	},
	resolve: {
		extensions: [".js", ".jsx"]
	},
	module: {
		rules: [
			{
				test: /\.(js|jsx)$/,
				exclude: /(node_modules)/,
				use: {
					loader: "babel-loader",
					options: {
						presets: [
							[
								"@babel/preset-env",
								{
									corejs: "3",
									useBuiltIns: "entry"
								}
							],
							"@babel/preset-react",
							{
								plugins: [
									"@babel/plugin-proposal-class-properties"
								]
							}
						]
					}
				}
			},
			{
				test: /\.css$/,
				// exclude: /(node_modules)/,
				use: ["style-loader", "css-loader"]
			}
		]
	}
};
