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

export const defPromotionType = {
	1: "First Deposit",
	2: "Re-Deposit",
	3: "FreeBet",
	4: "Special",
	5: "Wallet Bonus",
	// 6: "Re-Bet",
	// 7: "CashBet",
	// 8: "Advertising"
};

export const initPromotionType = [
	{ value: 1, displayText: defPromotionType[1] },
	{ value: 2, displayText: defPromotionType[2] },
	{ value: 3, displayText: defPromotionType[3] },
	{ value: 5, displayText: defPromotionType[5] },
	// { value: 6, displayText: defPromotionType[6] },
	// { value: 7, displayText: defPromotionType[7] },
	// { value: 8, displayText: defPromotionType[8] }
];

export const initWalletType = () => Object.values(defWalletType);
