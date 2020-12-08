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
const MainFinancialReport = lazy(() => import("./pages/Report/MainFinancialReport/index"));
const BannerManage = lazy(() => import("./pages/Marketing/BannerManage/index"));

export const defaultPage = "BannerManage";

export const layoutType = {
	N88: "N88",
	LAP: "LAP"
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
			"./css/common/css/common.css"
		],
		layout: layoutType.LAP
	},
	UserAuthoritySetting: {
		comp: (
			<Suspense fallback="loading...">
				<UserAuthoritySetting />
			</Suspense>
		),
		css: [
			"./css/common/fonts/main-font/style.css",
			"./css/common/css/common.css"
		],
		layout: layoutType.LAP
	},
	LicWinLostReport: {
		comp: (
			<Suspense fallback="loading...">
				<LicWinLostReport />
			</Suspense>
		),
		css: [
			"./css/css/pages/report/LicenseeWinLost.css",
			"./lib/Oneworks/jscalendar/calendar-blue.css"
		],
		layout: layoutType.N88
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
			"./lib/Oneworks/jscalendar/calendar-blue.css"
		],
		layout: layoutType.N88
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
			"./lib/bootstrap-multiselect/bootstrap-multiselect-modify.css"
		],
		layout: layoutType.N88
	},
	PaymentManagement: {
		comp: (
			<Suspense fallback="loading...">
				<PaymentManagement />
			</Suspense>
		),
		css: [
			"./lib/bootstrap/dist/css/bootstrap.min.css",
			"./css/css/main.css"
		],
		layout: layoutType.N88
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
			"./css/css/vendor/DateTimePicker/DateTimePicker.css"
		],
		layout: layoutType.N88
	},
	WithdrawalLimitAdjust: {
		comp: (
			<Suspense fallback="loading...">
				<WithdrawalLimitAdjust />
			</Suspense>
		),
		css: [
			"./lib/bootstrap/dist/css/bootstrap.min.css",
			"./css/css/main.css"
		],
		layout: layoutType.N88
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
			"./css/css/vendor/DateTimePicker/DateTimePicker.css"
		],
		layout: layoutType.N88
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
			"./css/css/pages/Marketing.css"
		],
		layout: layoutType.N88
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
			".css/css/page/CusInfo.css"
		],
		layout: layoutType.N88
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
			"./css/css/pages/Marketing.css"
		],
		layout: layoutType.N88
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
			"./css/css/pages/Marketing.css"
		],
		layout: layoutType.N88
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
			"./css/css/pages/Marketing.css"
		],
		layout: layoutType.N88
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
			"./css/css/pages/Marketing.css"
		],
		layout: layoutType.N88
	},
	DashBoard: {
		comp: (
			<Suspense fallback="loading...">
				<DashBoard />
			</Suspense>
		),
		css: [
			"./lib/bootstrap/dist/css/bootstrap.min.css",
			"./css/css/main.css"
		],
		layout: layoutType.N88
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
			"./css/css/pages/Marketing.css"
		],
		layout: layoutType.N88
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
			"./css/css/vendor/DateTimePicker/DateTimePicker.css"
		],
		layout: layoutType.N88
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
			"./css/css/vendor/DateTimePicker/DateTimePicker.css"
		],
		layout: layoutType.N88
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
			"./css/css/vendor/DateTimePicker/DateTimePicker.css"
		],
		layout: layoutType.N88
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
			"./css/css/vendor/DateTimePicker/DateTimePicker.css"
		],
		layout: layoutType.N88
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
			"./css/css/vendor/DateTimePicker/DateTimePicker.css"
		],
		layout: layoutType.N88
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
			"./css/common/fonts/main-font/style.css",
			"./css/css/vendor/DateTimePicker/DateTimePicker.css"
		],
		layout: layoutType.N88
	},
};
