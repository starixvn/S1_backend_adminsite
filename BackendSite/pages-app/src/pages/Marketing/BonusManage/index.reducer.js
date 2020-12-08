export const defActionType = {
	SET_PROMOTION_LIST: "SET_PROMOTION_LIST",
	GOTO_ADD_BONUS: "GOTO_ADD_BONUS",
	SET_PROMOTION: "SET_PROMOTION",
	EDIT_PROMOTION: "EDIT_PROMOTION",
	CLICK_BACK: "CLICK_BACK",
	UPDATE_PROMOTION_STATUS: "UPDATE_PROMOTION_STATUS",
	HANDEL_POPBOX: "HANDEL_POPBOX",
	CLONE_PROMOTION: "CLONE_PROMOTION",
	SET_CLONE_VALUE: "SET_CLONE_VALUE",
	SET_SITE_LIST: "SET_SITE_LIST",
	SET_STATUS:"SET_STATUS"
};

export const defPanelType = {
	Main: "Main",
	Edit: "Edit",
	Add: "Add"
};

export const initState = {
	promotionData: [],
	promotionList: [],
	siteList: [],
	panelType: defPanelType.Main,
	popBoxIsOpen: false,
	clonePromotionData: [],
	statusCheck: []
};

export function reducer(state, action) {
	switch (action.type) {
		case defActionType.SET_PROMOTION_LIST:
			return {
				...state,
				promotionList: action.promotionList
			};

		case defActionType.GOTO_ADD_BONUS:
			return {
				...state,
				siteList: action.siteList.sort((a, b) => {
					return a.siteId > b.siteId ? 1 : -1;
				}),
				panelType: defPanelType.Add
			};

		case defActionType.SET_PROMOTION:
			return {
				...state,
				promotionList: action.promotionList,
				panelType: defPanelType.Main,
				popBoxIsOpen: action.popBoxIsOpen
			};

		case defActionType.EDIT_PROMOTION:
			return {
				...state,
				promotionData: action.promotionData,
				siteList: action.siteList.sort((a, b) => {
					return a.siteId > b.siteId ? 1 : -1;
				}),
				panelType: defPanelType.Edit
			};

		case defActionType.CLICK_BACK:
			return {
				...state,
				panelType: defPanelType.Main
			};

		case defActionType.UPDATE_PROMOTION_STATUS:
			let newList = [...state.promotionList];
			newList[action.idx].status = action.status;
			newList[action.idx].lastUpdateUser = action.lastUpdateUser;
			newList[action.idx].lastUpdateTime = action.lastUpdateTime;
			return {
				...state,
				promotionList: newList
			};

		case defActionType.HANDEL_POPBOX:
			return {
				...state,
				popBoxIsOpen: action.popBoxIsOpen,
				promotionData: action.promotionData,
				clonePromotionData: { siteId: action.promotionData.siteId, bonusCode: action.promotionData.bonusCode }
			};

		case defActionType.SET_CLONE_VALUE:
			return {
				...state,
				clonePromotionData: { ...state.clonePromotionData, [action.dataColumn]: action.value }
			};

		case defActionType.CLONE_PROMOTION:
			return {
				...state,
				popBoxIsOpen: false,
				promotionList: action.promotionList
			};
		
		case defActionType.SET_SITE_LIST:
		return {
			...state,
			siteList: action.siteList
		};

		default:
			throw new Error();
	}
}
