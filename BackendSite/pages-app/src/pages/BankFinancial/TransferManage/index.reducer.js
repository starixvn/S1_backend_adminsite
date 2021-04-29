export const defActionType = {
  SET_SITE_LIST: "SET_SITE_LIST",
  SET_REASON_LIST: "SET_REASON_LIST",
  SET_TRANSFER_INFO_LIST: "SET_TRANSFER_INFO_LIST",
  GOTO_ADD_TRANSFER_INFO: "GOTO_ADD_TRANSFER_INFO",
  SET_TRANSFER_INFO: "SET_TRANSFER_INFO",
  EDIT_TRANSFER_INFO: "EDIT_TRANSFER_INFO",
  CLICK_BACK: "CLICK_BACK",
};

export const defPanelType = {
  Main: "Main",
  Edit: "Edit",
  Add: "Add",
};

export const initState = {
  transferInfoData: [],
  transferInfoList: [],
  reasonList: [],
  siteList: [],
  panelType: defPanelType.Main,
};

export function reducer(state, action) {
  switch (action.type) {
    case defActionType.SET_REASON_LIST:
      return {
        ...state,
        reasonList: action.reasonList,
      };
    case defActionType.SET_TRANSFER_INFO_LIST:
      return {
        ...state,
        transferInfoList: action.transferInfoList,
        setSite: action.setSite,
        setCurrencyList: action.setCurrencyList,
      };

    case defActionType.SET_TRANSFER_INFO:
      return {
        ...state,
        panelType: defPanelType.Main,
      };

    case defActionType.GOTO_ADD_TRANSFER_INFO:
      return {
        ...state,
        transferInfoData: action.transferInfoData,
        panelType: defPanelType.Add,
      };

    case defActionType.EDIT_TRANSFER_INFO:
      return {
        ...state,
        transferInfoData: action.transferInfoData,
        panelType: defPanelType.Edit,
      };

    case defActionType.CLICK_BACK:
      return {
        ...state,
        panelType: defPanelType.Main,
        transferInfoList: [],
      };

    default:
      throw new Error();
  }
}
