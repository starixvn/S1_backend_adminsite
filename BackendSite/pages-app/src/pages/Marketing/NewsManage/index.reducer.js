export const defActionType = {
	SET_DATA_LIST: "SET_DATA_LIST",
	SET_SITE_LIST: "SET_SITE_LIST",
	GOTO_ADD: "GOTO_ADD",
	CLICK_BACK: "CLICK_BACK",
	SET_STATUS: "SET_STATUS",
	GOTO_EDIT: "GOTO_EDIT"
};

export const defPanelType = {
	Main: "Main",
	Edit: "Edit",
	Add: "Add"
};

export const initState = {
	data: null,
	dataList: [],
	siteList: [],
	panelType: defPanelType.Main
};

export function reducer(state, action) {
	switch (action.type) {
		case defActionType.SET_DATA_LIST:
			return {
				...state,
				data: null,
				dataList: action.dataList,
				panelType: defPanelType.Main
			};

		case defActionType.SET_SITE_LIST:
			return {
				...state,
				siteList: action.siteList.sort((a, b) => {
					return a.siteId > b.siteId ? 1 : -1;
				})
			};

		case defActionType.GOTO_ADD:
			return {
				...state,
				panelType: defPanelType.Add
			};

		case defActionType.CLICK_BACK:
			return {
				...state,
				panelType: defPanelType.Main
			};

		case defActionType.SET_STATUS:
			let _dataList = [...state.dataList];
			_dataList[action.idx].status = action.status;
			_dataList[action.idx].lastUpdateUser = action.lastUpdateUser;
			_dataList[action.idx].lastUpdateTime = action.lastUpdateTime;

			return {
				...state,
				dataList: _dataList
			};

		case defActionType.GOTO_EDIT:
			return {
				...state,
				data: action.data,
				panelType: defPanelType.Edit
			};

		default:
			throw new Error();
	}
}
