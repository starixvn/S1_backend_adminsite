export const defActionType = {
	SET_PROMOTION_LIST: "SET_PROMOTION_LIST",
	GOTO_ADD_BONUS: "GOTO_ADD_BONUS",
	SET_PROMOTION: "SET_PROMOTION",
	EDIT_PROMOTION: "EDIT_PROMOTION",
	CLICK_BACK: "CLICK_BACK",
	UPDATE_PROMOTION_STATUS: "UPDATE_PROMOTION_STATUS"
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
	panelType: defPanelType.Main
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
				siteList: action.siteList,
				panelType: defPanelType.Add
			};

		case defActionType.SET_PROMOTION:
			return {
				...state,
				promotionList: action.promotionList,
				panelType: defPanelType.Main
			};

		case defActionType.EDIT_PROMOTION:
			return {
				...state,
				promotionData: action.promotionData,
				siteList: action.siteList,
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
			return {
				...state,
				promotionList: newList
			};

		default:
			throw new Error();
	}
}
