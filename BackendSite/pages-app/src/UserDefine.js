import React, { lazy, Suspense } from "react";

const LicWinLostReport = lazy(() =>
  import("./pages/Report/LicenseeWinLost/index")
);
const GroupAuthoritySetting = lazy(() =>
  import("./pages/Administration/GroupAuthoritySetting/index")
);
const UserAuthoritySetting = lazy(() =>
  import("./pages/Administration/UserAuthoritySetting/index")
);
const PaymentAdjustmentReport = lazy(() =>
  import("./pages/Finance/PaymentAdjustmentReport/index")
);
const BonusInfoReport = lazy(() =>
  import("./pages/Marketing/BonusInfoReport/index")
);
const PaymentManagement = lazy(() =>
  import("./pages/Finance/PaymentManagement/index")
);
const Announcement = lazy(() => import("./pages/CSD/Announcement/index"));
const WithdrawalLimitAdjust = lazy(() =>
  import("./pages/Finance/WithdrawalLimitAdjust/index")
);
const PersonalMsgBatch = lazy(() =>
  import("./pages/CSD/PersonalMsgBatch/index")
);
const PointLevelManagement = lazy(() =>
  import("./pages/Marketing/PointLevelManagement/index")
);
const CashBackBonus = lazy(() =>
  import("./pages/Marketing/CashBackBonus/index")
);
const DepositLimitManagement = lazy(() =>
  import("./pages/Finance/DepositLimitManagement/index")
);
const WithdrawalLimitManagement = lazy(() =>
  import("./pages/Finance/WithdrawalLimitManagement/index")
);
const ChangeVipLevelBatch = lazy(() =>
  import("./pages/CSD/ChangeVipLevelBatch/index")
);
const WithdrawalKYCControl = lazy(() =>
  import("./pages/CSD/WithdrawalKYCControl/index")
);
const DashBoard = lazy(() => import("./pages/Home/DashBoard/index"));
const BalanceAdjustmentBatch = lazy(() =>
  import("./pages/Finance/BalanceAdjustmentBatch/index")
);
const BonusManage = lazy(() => import("./pages/Marketing/BonusManage/index"));
const PendingBonus = lazy(() => import("./pages/Marketing/PendingBonus/index"));
const ThirdPartyAdjustment = lazy(() =>
  import("./pages/Finance/ThirdPartyAdjustment/index")
);
const NewsManage = lazy(() => import("./pages/Marketing/NewsManage/index"));
const MainFinancialReport = lazy(() =>
  import("./pages/Report/MainFinancialReport/index")
);
const BannerManage = lazy(() => import("./pages/Marketing/BannerManage/index"));
const StatisticalReport = lazy(() =>
  import("./pages/Report/StatisticalReport/index")
);
const UserOnlineReport = lazy(() =>
  import("./pages/Report/UserOnlineReport/index")
);
const BonusImport = lazy(() => import("./pages/Marketing/BonusImport/index"));
const DepositReport = lazy(() => import("./pages/Report/DepositReport/index"));
const ReferralManagement = lazy(() =>
  import("./pages/Marketing/ReferralManagement/index")
);
const ReferralReport = lazy(() =>
  import("./pages/Marketing/ReferralReport/index")
);
const DrawPrizeReport = lazy(() =>
  import("./pages/Marketing/DrawPrizeReport/index")
);
const CustomerStatisticalReport = lazy(() =>
  import("./pages/Report/CustomerStatisticalReport/index")
);
const GameCasinoManagement = lazy(() =>
  import("./pages/Marketing/GameCasinoManagement/index")
);
const TeleCSD = lazy(() => import("./pages/CSD/TeleCSD/index"));
const TransferAccountManage = lazy(() =>
  import("./pages/BankFinancial/TransferAccountManage/index")
);
const VipUpgrade = lazy(() => import("./pages/Marketing/VipUpgrade/index"));
const LatestHighlightVideoManagement = lazy(() =>
  import("./pages/Marketing/LatestHighlightVideoManagement/index")
);
const TransferManage = lazy(() =>
  import("./pages/BankFinancial/TransferManage/index")
);
const VipRebateInfoReport = lazy(() =>
  import("./pages/Marketing/VipRebateInfoReport/index")
);
const VipRebateMangetment = lazy(() =>
  import("./pages/Marketing/VipRebateMangetment/index")
);

const WithdrawalLimitManage = lazy(() =>
  import("./pages/Finance/WithdrawalLimitManage/index")
);

const DomainManage = lazy(() => import("./pages/Marketing/DomainManage/index"));

const DashBoardNew = lazy(() => import("./pages/Home/DashBoardNew/index"));

const TransferInfoReport = lazy(() =>
  import("./pages/BankFinancial/TransferInfoReport/index")
);

const BankFinancialReport = lazy(() =>
  import("./pages/BankFinancial/BankFinancialReport/index")
);

const WithdrawalAccounting = lazy(() =>
  import("./pages/Finance/WithdrawalAccounting/index")
);

const LeaderBoardExtraPoint = lazy(() =>
  import("./pages/Marketing/LeaderBoardExtraPoint/index")
);

const DisableUserForGame = lazy(() =>
  import("./pages/CSD/DisableUserForGame/index")
);

const VipLevelReview = lazy(() =>
  import("./pages/Marketing/VipLevelReview/index")
);

const UserBetLimit = lazy(() => import("./pages/CSD/UserBetLimit/index"));

const VerificationReward = lazy(() =>
  import("./pages/Marketing/VerificationReward/index")
);

const VerificationRewardReport = lazy(() =>
  import("./pages/Marketing/VerificationRewardReport/index")
);

const TpFailRecordReport = lazy(() =>
  import("./pages/CSD/TpFailRecordReport/index")
);

const CryptoWithdrawalLimitManagement = lazy(() =>
  import("./pages/Finance/CryptoWithdrawalLimitManagement/index")
);

export const defaultPage = "PersonalMsgBatch";

export const layoutType = {
  N88: "N88",
  LAP: "LAP",
};

export const pages = {
  GroupAuthoritySetting: {
    comp: (
      <Suspense fallback="loading...">
        <GroupAuthoritySetting />
      </Suspense>
    ),
    css: [
      "./css/common/fonts/main-font/style.css",
      "./css/common/css/common.css",
    ],
    layout: layoutType.LAP,
  },
  UserAuthoritySetting: {
    comp: (
      <Suspense fallback="loading...">
        <UserAuthoritySetting />
      </Suspense>
    ),
    css: [
      "./css/common/fonts/main-font/style.css",
      "./css/common/css/common.css",
    ],
    layout: layoutType.LAP,
  },
  LicWinLostReport: {
    comp: (
      <Suspense fallback="loading...">
        <LicWinLostReport />
      </Suspense>
    ),
    css: [
      "./css/css/pages/report/LicenseeWinLost.css",
      "./lib/Oneworks/jscalendar/calendar-blue.css",
    ],
    layout: layoutType.N88,
  },
  PaymentAdjustmentReport: {
    comp: (
      <Suspense fallback="loading...">
        <PaymentAdjustmentReport />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./lib/Oneworks/jscalendar/calendar-blue.css",
    ],
    layout: layoutType.N88,
  },
  BonusInfoReport: {
    comp: (
      <Suspense fallback="loading...">
        <BonusInfoReport />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./lib/bootstrap-multiselect/bootstrap-multiselect-modify.css",
    ],
    layout: layoutType.N88,
  },
  PaymentManagement: {
    comp: (
      <Suspense fallback="loading...">
        <PaymentManagement />
      </Suspense>
    ),
    css: ["./lib/bootstrap/dist/css/bootstrap.min.css", "./css/css/main.css"],
    layout: layoutType.N88,
  },
  Announcement: {
    comp: (
      <Suspense fallback="loading...">
        <Announcement />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  WithdrawalLimitAdjust: {
    comp: (
      <Suspense fallback="loading...">
        <WithdrawalLimitAdjust />
      </Suspense>
    ),
    css: ["./lib/bootstrap/dist/css/bootstrap.min.css", "./css/css/main.css"],
    layout: layoutType.N88,
  },
  PersonalMsgBatch: {
    comp: (
      <Suspense fallback="loading...">
        <PersonalMsgBatch />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  PointLevelManagement: {
    comp: (
      <Suspense fallback="loading...">
        <PointLevelManagement />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/pages/Marketing.css",
    ],
    layout: layoutType.N88,
  },
  CashBackBonus: {
    comp: (
      <Suspense fallback="loading...">
        <CashBackBonus />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/common/fonts/main-font/style.css",
      "./css/common/css/common.css",
      ".css/css/page/CusInfo.css",
    ],
    layout: layoutType.N88,
  },
  DepositLimitManagement: {
    comp: (
      <Suspense fallback="loading...">
        <DepositLimitManagement />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/pages/Marketing.css",
    ],
    layout: layoutType.N88,
  },
  WithdrawalLimitManagement: {
    comp: (
      <Suspense fallback="loading...">
        <WithdrawalLimitManagement />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/pages/Marketing.css",
    ],
    layout: layoutType.N88,
  },
  ChangeVipLevelBatch: {
    comp: (
      <Suspense fallback="loading...">
        <ChangeVipLevelBatch />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/pages/Marketing.css",
    ],
    layout: layoutType.N88,
  },
  WithdrawalKYCControl: {
    comp: (
      <Suspense fallback="loading...">
        <WithdrawalKYCControl />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/pages/Marketing.css",
    ],
    layout: layoutType.N88,
  },
  DashBoard: {
    comp: (
      <Suspense fallback="loading...">
        <DashBoard />
      </Suspense>
    ),
    css: ["./lib/bootstrap/dist/css/bootstrap.min.css", "./css/css/main.css"],
    layout: layoutType.N88,
  },
  BalanceAdjustmentBatch: {
    comp: (
      <Suspense fallback="loading...">
        <BalanceAdjustmentBatch />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/pages/Marketing.css",
    ],
    layout: layoutType.N88,
  },
  BonusManage: {
    comp: (
      <Suspense fallback="loading...">
        <BonusManage />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/common/fonts/main-font/style.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  PendingBonus: {
    comp: (
      <Suspense fallback="loading...">
        <PendingBonus />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  ThirdPartyAdjustment: {
    comp: (
      <Suspense fallback="loading...">
        <ThirdPartyAdjustment />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  NewsManage: {
    comp: (
      <Suspense fallback="loading...">
        <NewsManage />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  MainFinancialReport: {
    comp: (
      <Suspense fallback="loading...">
        <MainFinancialReport />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  BannerManage: {
    comp: (
      <Suspense fallback="loading...">
        <BannerManage />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  DomainManage: {
    comp: (
      <Suspense fallback="loading...">
        <DomainManage />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  StatisticalReport: {
    comp: (
      <Suspense fallback="loading...">
        <StatisticalReport />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  UserOnlineReport: {
    comp: (
      <Suspense fallback="loading...">
        <UserOnlineReport />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/common/fonts/main-font/style.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  BonusImport: {
    comp: (
      <Suspense fallback="loading...">
        <BonusImport />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  DepositReport: {
    comp: (
      <Suspense fallback="loading...">
        <DepositReport />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  ReferralManagement: {
    comp: (
      <Suspense fallback="loading...">
        <ReferralManagement />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  ReferralReport: {
    comp: (
      <Suspense fallback="loading...">
        <ReferralReport />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  DrawPrizeReport: {
    comp: (
      <Suspense fallback="loading...">
        <DrawPrizeReport />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  CustomerStatisticalReport: {
    comp: (
      <Suspense fallback="loading...">
        <CustomerStatisticalReport />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  GameCasinoManage: {
    comp: (
      <Suspense fallback="loading...">
        <GameCasinoManagement />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  TeleCSD: {
    comp: (
      <Suspense fallback="loading...">
        <TeleCSD />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  TransferAccountManage: {
    comp: (
      <Suspense fallback="loading...">
        <TransferAccountManage />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  VipUpgrade: {
    comp: (
      <Suspense fallback="loading...">
        <VipUpgrade />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  LatestHighlightVideoManagement: {
    comp: (
      <Suspense fallback="loading...">
        <LatestHighlightVideoManagement />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  TransferManage: {
    comp: (
      <Suspense fallback="loading...">
        <TransferManage />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  VipRebateInfoReport: {
    comp: (
      <Suspense fallback="loading...">
        <VipRebateInfoReport />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  VipRebateMangetment: {
    comp: (
      <Suspense fallback="loading...">
        <VipRebateMangetment />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  WithdrawalLimitManage: {
    comp: (
      <Suspense fallback="loading...">
        <WithdrawalLimitManage />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  DashBoardNew: {
    comp: (
      <Suspense fallback="loading...">
        <DashBoardNew />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  TransferInfoReport: {
    comp: (
      <Suspense fallback="loading...">
        <TransferInfoReport />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  BankFinancialReport: {
    comp: (
      <Suspense fallback="loading...">
        <BankFinancialReport />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  WithdrawalAccounting: {
    comp: (
      <Suspense fallback="loading...">
        <WithdrawalAccounting />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  DisableUserForGame: {
    comp: (
      <Suspense fallback="loading...">
        <DisableUserForGame />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/common/fonts/main-font/style.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  LeaderBoardExtraPoint: {
    comp: (
      <Suspense fallback="loading...">
        <LeaderBoardExtraPoint />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  VipLevelReview: {
    comp: (
      <Suspense fallback="loading...">
        <VipLevelReview />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  UserBetLimit: {
    comp: (
      <Suspense fallback="loading...">
        <UserBetLimit />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/common/fonts/main-font/style.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  VerificationReward: {
    comp: (
      <Suspense fallback="loading...">
        <VerificationReward />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/pages/Marketing.css",
    ],
    layout: layoutType.N88,
  },
  VerificationRewardReport: {
    comp: (
      <Suspense fallback="loading...">
        <VerificationRewardReport />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/pages/Marketing.css",
      "./css/common/fonts/main-font/style.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  TpFailRecordReport: {
    comp: (
      <Suspense fallback="loading...">
        <VerificationReward />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/pages/Marketing.css",
    ],
    layout: layoutType.N88,
  },
  VerificationRewardReport: {
    comp: (
      <Suspense fallback="loading...">
        <VerificationRewardReport />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/pages/Marketing.css",
      "./css/common/fonts/main-font/style.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  TpFailRecordReport: {
    comp: (
      <Suspense fallback="loading...">
        <TpFailRecordReport />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css",
      "./css/css/vendor/DateTimePicker/DateTimePicker.css",
    ],
    layout: layoutType.N88,
  },
  CryptoWithdrawalLimitManagement: {
    comp: (
      <Suspense fallback="loading...">
        <CryptoWithdrawalLimitManagement />
      </Suspense>
    ),
    css: [
      "./lib/bootstrap/dist/css/bootstrap.min.css",
      "./css/css/main.css"
    ],
    layout: layoutType.N88,
  }
};
