export const defActionType = {
	SET_SITE_LIST: "SET_SITE_LIST",
	SET_BANNER_LIST: "SET_BANNER_LIST",
	GOTO_ADD_BANNER: "GOTO_ADD_BANNER",
	SET_BANNER: "SET_BANNER",
	EDIT_BANNER: "EDIT_BANNER",
	CLICK_BACK: "CLICK_BACK",
};

export const defPanelType = {
	Main: "Main",
	Edit: "Edit",
	Add: "Add"
};

export const initState = {
	bannerData: [],
	bannerList: [],
	siteList: [],
	setSite: "",
	setCurrencyList: [],
	setLangList: [],
	panelType: defPanelType.Main
};

export function reducer(state, action) {
	switch (action.type) {
		case defActionType.SET_SITE_LIST:
			return {
				...state,
				siteList: action.siteList
			};
		case defActionType.SET_BANNER_LIST:
			return {
				...state,
				bannerList: action.bannerList,
				setSite: action.setSite,
				setCurrencyList: action.setCurrencyList,
				setLangList: action.setLangList
			};

		case defActionType.GOTO_ADD_BANNER:
			return {
				...state,
				panelType: defPanelType.Add
			};

		case defActionType.SET_BANNER:
			return {
				...state,
				panelType: defPanelType.Main,
			};

		case defActionType.EDIT_BANNER:
			return {
				...state,
				bannerData: action.bannerData,
				panelType: defPanelType.Edit
			};

		case defActionType.CLICK_BACK:
			return {
				...state,
				panelType: defPanelType.Main
			};

		default:
			throw new Error();
	}
}
