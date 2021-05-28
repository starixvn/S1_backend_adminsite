export const defActionType = {
	SET_INIT_STATE: "SET_INIT_STATE",
	GET_SITE_INFO: "GET_SITE_INFO",
	GET_REBATE_PERIOD: "GET_REBATE_PERIOD",
	GET_REBATE_INFO: "GET_REBATE_INFO",
	GET_REBATE_INFO_BY_TPCATEGORY: "GET_REBATE_INFO_BY_TPCATEGORY",
	GET_REBATE_INFO_BY_GAME_TYPE: "GET_REBATE_INFO_BY_GAME_TYPE",

	SET_EDIT: "SET_EDIT",
	SET_DATA: "SET_DATA",

};

export const initState = {
	initEdit: {},
	initSiteList: [],
	initTpCategoryList: [],

	siteCurrencyList: [],
	siteVipList: [],
	tpGameTypeListbyCategory: [],

	currencyTab: 0,
	tpCategoryTab: 0,
	tpGameTypeTab: 0,

	setSiteData: 0,
	setRebatePeriodData: {},
	setCurrencyData: {},
	setCategoryPayoutData: {},
	setCategoryData: {},
	setGameTypeData: {},

	setData: {}

};

export function reducer(state, action) {
	switch (action.type) {
		case defActionType.SET_INIT_STATE:
			return {
				...state,
				initEdit: {},
				[action.state]: action.data
			};
		case defActionType.GET_SITE_INFO:
			return {
				...state,
				initEdit: {},
				siteCurrencyList: action.siteCurrencyList,
				setSiteData: action.site,
				currencyTab: 0,
				tpCategoryTab: 0,
				setRebatePeriodData: {},
				setCurrencyData: {},
				setCategoryPayoutData: {},
				setCategoryData: {},
			};
		case defActionType.GET_REBATE_PERIOD:
			return {
				...state,
				initEdit: {},
				initCurrencyList: action.initCurrencyList,
				setSiteData: action.setSite,
				setRebatePeriodData: action.setRebatePeriod,
			};
		case defActionType.GET_REBATE_INFO:
			return {
				...state,
				initEdit: {},
				currencyTab: action.setCurrencyTab,
				setCurrencyData: action.setCurrencyData,
				tpCategoryTab: 0,
				setCategoryPayoutData: {},
				setCategoryData: {},
			};
		case defActionType.GET_REBATE_INFO_BY_TPCATEGORY:
			return {
				...state,
				initEdit: {},
				tpCategoryTab: action.setTpCategoryTab,
				setCategoryPayoutData: action.setCategoryPayoutData,
				setCategoryData: action.setCategoryData,
				tpGameTypeListbyCategory: action.settpGameTypeListbyCategory,
				tpGameTypeTab: 0,
				setGameTypeData: {},
			};
		case defActionType.GET_REBATE_INFO_BY_GAME_TYPE:
			return {
				...state,
				initEdit: {},
				tpGameTypeTab: action.setTpGameTypeTab,
				setGameTypeData: action.setGameTypeData
			};
		case defActionType.SET_EDIT:
			let data = action.originalData ? action.originalData : {}
			return {
				...state,
				initEdit: {
					[action.idx]: action.val
				},
				setData: data
			};
		case defActionType.SET_DATA:
			return {
				...state,
				setData: {
					...state.setData,
					[action.state]: action.data
				}
			};

		default:
			throw new Error();
	}
}