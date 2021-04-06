export const defActionType = {
  SET_SITE_LIST: "SET_SITE_LIST",
  SET_TRANSFER_ACCOUNT_LIST: "SET_TRANSFER_ACCOUNT_LIST",
  GOTO_ADD_TRANSFER_ACCOUNT: "GOTO_ADD_TRANSFER_ACCOUNT",
  SET_TRANSFER_ACCOUNT: "SET_TRANSFER_ACCOUNT",
  EDIT_TRANSFER_ACCOUNT: "EDIT_TRANSFER_ACCOUNT",
  CLICK_BACK: "CLICK_BACK",
};

export const defPanelType = {
  Main: "Main",
  Edit: "Edit",
  Add: "Add",
};

export const initState = {
  transferAccountData: [],
  transferAccountList: [],
  siteList: [],
  setSite: "",
  setCurrencyList: [],
  panelType: defPanelType.Main,
};

export function reducer(state, action) {
  switch (action.type) {
    case defActionType.SET_SITE_LIST:
      return {
        ...state,
        siteList: action.siteList,
      };
    case defActionType.SET_TRANSFER_ACCOUNT_LIST:
      return {
        ...state,
        transferAccountList: action.transferAccountList,
        setSite: action.setSite,
        setCurrencyList: action.setCurrencyList,
      };

    case defActionType.SET_TRANSFER_ACCOUNT:
      return {
        ...state,
        panelType: defPanelType.Main,
      };

    case defActionType.GOTO_ADD_TRANSFER_ACCOUNT:
      return {
        ...state,
        transferAccountData: action.transferAccountData,
        panelType: defPanelType.Add,
      };

    case defActionType.EDIT_TRANSFER_ACCOUNT:
      return {
        ...state,
        transferAccountData: action.transferAccountData,
        panelType: defPanelType.Edit,
      };

    case defActionType.CLICK_BACK:
      return {
        ...state,
        panelType: defPanelType.Main,
      };

    default:
      throw new Error();
  }
}
