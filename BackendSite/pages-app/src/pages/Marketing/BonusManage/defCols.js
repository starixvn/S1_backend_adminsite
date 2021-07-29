export const defWalletType = {
	0: {
		value: 0,
		displayText: "Transfer"
	},
	1: {
		value: 1,
		displayText: "Seamless"
	}
};

export const initWalletType = () => Object.values(defWalletType);

export const defPromotionType = {
	1: "First Deposit",
	2: "Re-Deposit",
	3: "FreeBet",
	4: "Special",
	5: "Wallet Bonus",
	6: "Rebate",
	7: "CashBack",
	8: "Static page",
};

export const initPromotionType = [
	{ value: 1, displayText: defPromotionType[1] },
	{ value: 2, displayText: defPromotionType[2] },
	{ value: 3, displayText: defPromotionType[3] },
	{ value: 5, displayText: defPromotionType[5] },
	{ value: 6, displayText: defPromotionType[6] },
	{ value: 7, displayText: defPromotionType[7] },
	{ value: 8, displayText: defPromotionType[8] }
];

export const initSpecialBonusPromotionType = [
	{ value: 1, displayText: defPromotionType[1] },
	{ value: 3, displayText: defPromotionType[3] },
];

export const specialBonusOptions = [
	{ value: 1, displayText: "SpecialBonus"}
];

export const onlyRegisteredBonusOptions = [
	{ value: 1, displayText: "New Member"}
];
export const AutoExtendOptions = [
	{ value: 1, displayText: "Auto Extend"}
];

export const periodType = {
	1: "Daily",
	2: "Weekly",
	3: "Monthly",
}

export const initPeriodType = [
	{ value: 1, displayText: periodType[1] },
	{ value: 2, displayText: periodType[2] },
	{ value: 3, displayText: periodType[3] }
];

export const weekOptions = {
	1: 'Monday',
	2: 'Tuesday',
	3: 'Wednesday',
	4: 'Thursday',
	5: 'Friday',
	6: 'Saturday',
	7: 'Sunday'
}

export const initWeekOptions = [
	{ value: 1, displayText: weekOptions[1] },
	{ value: 2, displayText: weekOptions[2] },
	{ value: 3, displayText: weekOptions[3] },
	{ value: 4, displayText: weekOptions[4] },
	{ value: 5, displayText: weekOptions[5] },
	{ value: 6, displayText: weekOptions[6] },
	{ value: 7, displayText: weekOptions[7] }
];

export const defPromotionDeviceType = {
	1: "All",
	2: "Web",
	3: "App"
};

export const initPromotionDeviceType = [
	{ value: 1, displayText: defPromotionDeviceType[1] },
	{ value: 2, displayText: defPromotionDeviceType[2] },
	{ value: 3, displayText: defPromotionDeviceType[3] }
];

export const defPromotionAction = {
	0: {
		value: 0,
		displayText: "Link"
	},
	1: {
		value: 1,
		displayText: "Detail"
	}
};

export const initStatusType = [
	{ value: 0, displayText: "All" },
	{ value: 1, displayText: "Active" },
	{ value: 2, displayText: "Inactive" }
];

export const initDeviceType = [
	{ value: 0, displayText: "Select All" },
	{ value: 1, displayText: "All" },
	{ value: 2, displayText: "Web" },
	{ value: 3, displayText: "App" }
];
export const initFilterType = [
	{ value: 0, displayText: "All" },
	{ value: 1, displayText: "New" },
	{ value: 2, displayText: "Used" }
];
export const initPromotionAction = () => Object.values(defPromotionAction);
