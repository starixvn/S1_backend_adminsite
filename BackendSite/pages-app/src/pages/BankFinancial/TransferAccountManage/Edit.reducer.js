export const defActionType = {
  MAIN_SET_VALUE: "MAIN_SET_VALUE",
  CURRENCY_SET_VALUE: "CURRENCY_SET_VALUE",
  CHANNEL_SET_VALUE: "CHANNEL_SET_VALUE",
  PURPOSE_SET_VALUE: "PURPOSE_SET_VALUE",
  PURPOSE_SET_VALUE_ALL: "PURPOSE_SET_VALUE_ALL",
  REASON_SET_LIST: "REASON_SET_LIST",
  PAY_SET_LIST: "PAY_SET_LIST",
  BANK_CODE_SET_LIST: "BANK_CODE_SET_LIST",
  MAIN_SET_SITEID_VALUE: "MAIN_SET_SITEID_VALUE",
  MAIN_SET_CURRENCY_VALUE: "MAIN_SET_CURRENCY_VALUE",
  MAIN_SET_CHANNEL_VALUE: "MAIN_SET_CHANNEL_VALUE",
  MAIN_SET_PURPOSE_CHECKBOX: "MAIN_SET_PURPOSE_CHECKBOX",
};

export const initialState = (datas, setCurrencyList, isAdd) => {
  let _selectedPurpposes = [];
  if (!isAdd) {
    _selectedPurpposes = datas.purpose.split(",").map((str) => Number(str));
  }
  if (isAdd) {
    switch (datas.siteId) {
      case 0:
        datas.siteName = "Soon88";
        break;
      case 1:
        datas.siteName = "Nova88";
        break;
      case 2:
        datas.siteName = "Goal123";
        break;
      case 3:
        datas.siteName = "Bwing";
        break;
    }
  }
  return {
    id: datas.id,
    siteId: datas.siteId,
    siteName: datas.siteName,
    currencyId: datas.currencyId,
    sysId: datas.sysId,
    purpose: _selectedPurpposes,
    channel: datas.channel,
    agent: datas.agent,
    payId: datas.payId,
    accountNo: datas.accountNo,
    accountName: datas.accountName,
    displayName: datas.displayName,
    initialAmount: datas.initialAmount,
    maxBalanceAlertAmount: datas.maxBalanceAlertAmount,
    reason: datas.reason,
    remark: datas.remark,
    status: datas.status,
    createTime: datas.createTime,
    creator: datas.creator,
    creatorName: datas.creatorName,
    updateTime: datas.updateTime,
    updator: datas.updator,
    updatorName: datas.updatorName,
    currencyList: setCurrencyList,
    reasonList: [],
    payList: [],
    bankCodeList: [],
    setBankCodeList: [],
  };
};

export function reducer(state, action) {
  switch (action.type) {
    case defActionType.MAIN_SET_VALUE:
      return {
        ...state,
        [action.dataColumn]: action.value,
      };

    case defActionType.MAIN_SET_SITEID_VALUE:
      if (action.value == 0) {
        return {
          ...state,
          [action.dataColumn]: action.value,
          siteName: "Soon88",
        };
      } else if (action.value == 1) {
        return {
          ...state,
          [action.dataColumn]: action.value,
          siteName: "Nova88",
        };
      } else if (action.value == 2) {
        return {
          ...state,
          [action.dataColumn]: action.value,
          siteName: "Goal123",
        };
      } else if (action.value == 3) {
        return {
          ...state,
          [action.dataColumn]: action.value,
          siteName: "Bwing",
        };
      } else {
        return {
          ...state,
          [action.dataColumn]: action.value,
          siteName: "",
        };
      }

    case defActionType.MAIN_SET_CURRENCY_VALUE:
      let currencyBankList = [];
      currencyBankList.push({
        value: 0,
        displayText: "- Select Bank Code -",
      });
      state.bankCodeList.map((s) => {
        if (s.currencyId == action.value) {
          currencyBankList.push({
            value: s.value,
            currencyId: s.currencyId,
            displayText: s.displayText,
          });
        }
      });
      return {
        ...state,
        [action.dataColumn]: action.value,
        sysId: 0,
        setBankCodeList: currencyBankList,
      };

    case defActionType.MAIN_SET_CHANNEL_VALUE:
      if (action.value == 3) {
        return {
          ...state,
          [action.dataColumn]: action.value,
          agent: "",
        };
      } else {
        return {
          ...state,
          [action.dataColumn]: action.value,
          payId: 0,
        };
      }

    case defActionType.CURRENCY_SET_VALUE:
      return {
        ...state,
        specific: {
          ...state.specific,
          [action.currencyId]: {
            ...state.specific[action.currencyId],
            [action.dataColumn]: action.value,
          },
        },
      };

    case defActionType.MAIN_SET_PURPOSE_CHECKBOX:
      let isRemove = state[action.dataColumn].includes(action.value);
      let newCurrencies = isRemove
        ? state[action.dataColumn].filter((v) => v !== action.value)
        : [...state[action.dataColumn], action.value];

      return {
        ...state,
        [action.dataColumn]: newCurrencies,
      };

    case defActionType.REASON_SET_LIST:
      let resultList = [];
      resultList.push({ value: 0, displayText: "- Select Reason -" });
      action.reasonList.map((s) => {
        resultList.push({
          value: s.reasonId,
          displayText: s.description,
        });
      });
      return {
        ...state,
        [action.dataColumn]: action.value,
        reasonList: resultList,
      };

    case defActionType.BANK_CODE_SET_LIST:
      let bankList = [];
      let defaultBankList = [];
      defaultBankList.push({
        value: 0,
        displayText: "- Select Bank Code -",
      });
      action.bankCodeList.map((s) => {
        bankList.push({
          value: s.sysId,
          currencyId: s.currencyId,
          displayText: s.bankCode + " - " + s.bankName,
        });
      });
      if (state.currencyId != 0) {
        bankList.map((s) => {
          if (s.currencyId == state.currencyId) {
            defaultBankList.push({
              value: s.value,
              currencyId: s.currencyId,
              displayText: s.displayText,
            });
          }
        });
      }
      return {
        ...state,
        [action.dataColumn]: action.value,
        bankCodeList: bankList,
        setBankCodeList: defaultBankList,
      };

    case defActionType.PAY_SET_LIST:
      let setPayList = [];
      setPayList.push({ value: 0, displayText: "- Select Pay -" });
      action.payList.map((s) => {
        setPayList.push({
          value: s.id,
          displayText: s.payName,
        });
      });
      return {
        ...state,
        [action.dataColumn]: action.value,
        payList: setPayList,
      };

    default:
      throw new Error();
  }
}
