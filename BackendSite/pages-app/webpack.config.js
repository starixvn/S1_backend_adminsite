const path = require("path");
//const webpack = require('webpack');
//const bundleAnalyzer = require("webpack-bundle-analyzer").BundleAnalyzerPlugin;

module.exports = {
  mode: "production",
  optimization: {
    mangleWasmImports: true,
    nodeEnv: "production",
  },
  externals: {
    react: "React",
    "react-dom": "ReactDOM",
    moment: "moment",
    jquery: "$",
    axios: "axios",
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
    //   // '@babel/polyfill',
    //   "./src/pages/Report/LicenseeWinLost/index.js",
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
    "CSD/Announcement.min": ["./src/pages/CSD/Announcement/index.js"],
    // "Finance/WithdrawalLimitAdjust.min": [
    //   "./src/pages/Finance/WithdrawalLimitAdjust/index.js",
    // ],
    // "CSD/PersonalMsgBatch.min": [
    // 	"./src/pages/CSD/PersonalMsgBatch/index.js"
    // ],
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
    //   "@babel/polyfill",
    //   "./src/pages/Marketing/BonusManage/index.jsx",
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
    //   "./src/pages/Report/MainFinancialReport/index.js",
    // ],
    // "Marketing/BannerManage.min": [
    //   '@babel/polyfill',
    //   "./src/pages/Marketing/BannerManage/index.jsx"
    // ],
    // "Report/MainFinancialReport.min": [
    //   "./src/pages/Report/MainFinancialReport/index.js",
    // ],
    // "Report/StatisticalReport.min": [
    //   "./src/pages/Report/StatisticalReport/index.js",
    // ],
    // "Report/UserOnlineReport.min": [
    //   "./src/pages/Report/UserOnlineReport/index.js",
    // ],
    // "Report/DepositReport.min": [
    //     "./src/pages/Report/DepositReport/index.js"
    // ],
    // "CSD/TpFailRecordReport.min": [
    //     "./src/pages/CSD/TpFailRecordReport/index.js"
    // ],
    // "Marketing/BonusImport.min": [
    // 	"@babel/polyfill",
    // 	"./src/pages/Marketing/BonusImport/index.js"
    // ],
    // "Marketing/ReferralReport.min": [
    // 	"@babel/polyfill",
    // 	"./src/pages/Marketing/ReferralReport/index.js"
    // ],
    // "Marketing/ReferralManagement.min": [
    // 	"@babel/polyfill",
    // 	"./src/pages/Marketing/ReferralManagement/index.js"
    // ],
    // "Marketing/DrawPrizeReport.min": [
    // 	"@babel/polyfill",
    // 	"./src/pages/Marketing/DrawPrizeReport/index.js"
    // ],
    // "Report/CustomerStatisticalReport.min": [
    // 	"./src/pages/Report/CustomerStatisticalReport/index.js"
    // ],
    // "Marketing/GameCasinoManagement.min": [
    // 	"@babel/polyfill",
    // 	"./src/pages/Marketing/GameCasinoManagement/index.js"
    // ]
    // "CSD/TeleCSD.min": ["@babel/polyfill", "./src/pages/CSD/TeleCSD/index.js"],
    // "Marketing/VipUpgrade.min": [
    // 	"@babel/polyfill",
    // 	"./src/pages/Marketing/VipUpgrade/index.js"
    // ],
    // "Marketing/LatestHighlightVideoManagement.min": [
    // 	"@babel/polyfill",
    // 	"./src/pages/Marketing/LatestHighlightVideoManagement/index.js"
    // ],
    // "BankFinancial/TransferAccountManage.min": [
    //   "@babel/polyfill",
    //   "./src/pages/BankFinancial/TransferAccountManage/index.jsx",
    // ],
    // "Marketing/VipRebateInfoReport.min": [
    //   "@babel/polyfill",
    //   "./src/pages/Marketing/VipRebateInfoReport/index.js",
    // ],
    // "Marketing/VipRebateMangetment.min": [
    //   '@babel/polyfill',
    //   "./src/pages/Marketing/VipRebateMangetment/index.jsx"
    // ],
    // "Finance/WithdrawalLimitManage.min": [
    //   // '@babel/polyfill',
    //   "./src/pages/Finance/WithdrawalLimitManage/index.js",
    // ],
    // "BankFinancial/TransferManage.min": [
    //   "@babel/polyfill",
    //   "./src/pages/BankFinancial/TransferManage/index.jsx",
    // ],
    // "Marketing/VipRebateInfoReport.min": [
    //   "@babel/polyfill",
    //   "./src/pages/Marketing/VipRebateInfoReport/index.js",
    // ],

    // "Marketing/DomainManage.min": [
    // 	"./src/pages/Marketing/DomainManage/index.js"
    // ],
    // "Finance/WithdrawalLimitManage.min": [
    // 	// '@babel/polyfill',
    // 	"./src/pages/Finance/WithdrawalLimitManage/index.js"
    // ],
    // "Home/DashBoardNew.min": ["./src/pages/Home/DashBoardNew/index.js"],
    // "BankFinancial/TransferInfoReport.min": [
    //   "@babel/polyfill",
    //   "./src/pages/BankFinancial/TransferInfoReport/index.js",
    // ],
    // "BankFinancial/BankFinancialReport.min": [
    //   "@babel/polyfill",
    //   "./src/pages/BankFinancial/BankFinancialReport/index.js",
    // ],
    //"Finance/DepositTimeReport.min": [
    // // '@babel/polyfill',
    //"./src/pages/Finance/DepositTimeReport/index.js",
    //],
    //"Finance/WithdrawalTimeReport.min": [
    //   //'@babel/polyfill',
    //"./src/pages/Finance/WithdrawalTimeReport/index.js",
    //],
    // "CSD/TagManagement.min": [
    //   '@babel/polyfill',
    // "./src/pages/CSD/TagManagement/index.js",
    // ],
    // "Finance/AdjustmentLimitManagement.min": [
    //   // '@babel/polyfill',
    //   "./src/pages/Finance/AdjustmentLimitManagement/index.js",
    // ],
    // "Administration/SpecialPasswordManagement.min": [
    // 	// '@babel/polyfill',
    // 	"./src/pages/Administration/SpecialPasswordManagement/index.js"
    // ],
    // "Finance/AdjustmentLogReport.min": [
    //   // '@babel/polyfill',
    //   "./src/pages/Finance/AdjustmentLogReport/index.js",
    // ],
    //"Finance/AdjustmentLogReport.min": [
    //  // '@babel/polyfill',
    //  "./src/pages/Finance/AdjustmentLogReport/index.js",
    //],
    // "Finance/WithdrawalAccounting.min": [
    //   "@babel/polyfill",
    //   "./src/pages/Finance/WithdrawalAccounting/index.js",
    // ],
    // "CSD/RegistrationManagement.min": [
    //   '@babel/polyfill',
    // "./src/pages/CSD/RegistrationManagement/index.js",
    // ],
    // "CSD/DisableUserForGame.min": [
    //   '@babel/polyfill',
    // "./src/pages/CSD/DisableUserForGame/index.js"
    // ],
    // "Marketing/LeaderBoardExtraPoint.min": [
    //   "@babel/polyfill",
    //   "./src/pages/Marketing/LeaderBoardExtraPoint/index.js",
    // ],
    "Marketing/VipLevelReview.min": [
      "@babel/polyfill",
      "./src/pages/Marketing/VipLevelReview/index.js",
    ],
    // "Marketing/LeaderBoardExtraPoint.min": [
    // 	 '@babel/polyfill',
    // 	"./src/pages/Marketing/LeaderBoardExtraPoint/index.js"
    // ],
    // "Marketing/AppLinkManagement.min": [
    // 	 '@babel/polyfill',
    // 	"./src/pages/Marketing/AppLinkManagement/index.js"
    // ],
    // "CSD/UserBetLimit.min": [
    //   "@babel/polyfill",
    //   "./src/pages/CSD/UserBetLimit/index.js"
    // ],
    // "Finance/USDTWithdrawalLimitManagement.min": [
    //   "./src/pages/Finance/USDTWithdrawalLimitManagement/index.js",
    // ],
    // "Marketing/VerificationReward.min": [
    //   "@babel/polyfill",
    //   "./src/pages/Marketing/VerificationReward/index.js",
    // ],
    "Marketing/VerificationRewardReport.min": [
      "@babel/polyfill",
      "./src/pages/Marketing/VerificationRewardReport/index.js",
    ],
  },
  output: {
    path: path.resolve(__dirname, "../BackendSite/wwwroot/js/component/"),
    filename: "[name].js",
  },
  resolve: {
    extensions: [".js", ".jsx"],
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
                  useBuiltIns: "entry",
                },
              ],
              "@babel/preset-react",
              {
                plugins: ["@babel/plugin-proposal-class-properties"],
              },
            ],
          },
        },
      },
      {
        test: /\.css$/,
        // exclude: /(node_modules)/,
        use: ["style-loader", "css-loader"],
      },
    ],
  },
};
