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
	5: "Wallet Bonus"
};

export const initPromotionType = [
	{ value: 1, displayText: defPromotionType[1] },
	//{ value: 2, displayText: "Re-Deposit" },
	{ value: 3, displayText: defPromotionType[3] },
	{ value: 5, displayText: defPromotionType[5] }
];

export const initWalletType = () => Object.values(defWalletType);
