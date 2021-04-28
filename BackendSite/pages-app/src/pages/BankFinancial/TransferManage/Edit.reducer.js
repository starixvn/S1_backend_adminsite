export const defActionType = {
  MAIN_SET_VALUE: "MAIN_SET_VALUE",

  MAIN_SET_FROM_CHANNEL_VALUE: "MAIN_SET_FROM_CHANNEL_VALUE",
  MAIN_SET_FROM_SITEID_VALUE: "MAIN_SET_FROM_SITEID_VALUE",
  MAIN_SET_FROM_CURRENCY_VALUE: "MAIN_SET_FROM_CURRENCY_VALUE",

  MAIN_SET_TO_CHANNEL_VALUE: "MAIN_SET_TO_CHANNEL_VALUE",
  MAIN_SET_TO_SITEID_VALUE: "MAIN_SET_TO_SITEID_VALUE",
  MAIN_SET_TO_CURRENCY_VALUE: "MAIN_SET_TO_CURRENCY_VALUE",

  //Initial List
  SITE_SET_LIST: "SITE_SET_LIST",
  FROM_CURRENCY_SET_LIST: "FROM_CURRENCY_SET_LIST",
  TO_CURRENCY_SET_LIST: "TO_CURRENCY_SET_LIST",
  FROM_BANK_SET_LIST: "FROM_BANK_SET_LIST",
  TO_BANK_SET_LIST: "TO_BANK_SET_LIST",
  CHANNEL_SET_VALUE: "CHANNEL_SET_VALUE",
  PAY_SET_LIST: "PAY_SET_LIST",
  BANK_CODE_SET_LIST: "BANK_CODE_SET_LIST",
  REASON_SET_LIST: "REASON_SET_LIST",
};

export const initialState = (
  datas,
  setFromSiteList,
  setFromCurrencyList,
  setFromBankList,
  setToSiteList,
  setToCurrencyList,
  setToBankList,
  isAdd
) => {
  if (isAdd) {
  }

  console.log("datas", datas);

  return {
    transferId: datas.transferId,
    transferType: datas.transferType,
    transDate: datas.transDate,

    //From Data
    fromChannel: datas.fromChannel,
    fromAgent: datas.fromAgent,
    fromPayId: datas.fromPayId,
    fromSiteId: datas.fromSiteId,
    fromSiteName: datas.fromSiteName,
    fromCustId: datas.fromCustId,
    fromCurrencyId: datas.fromCurrencyId,
    fromSysId: datas.fromSysId,
    fromAccountNo: datas.fromAccountNo,
    fromAccountDisplayName: datas.fromAccountDisplayName,
    fromAmount: datas.fromAmount,
    fromFee: datas.fromFee,

    //To Data
    toChannel: datas.toChannel,
    toAgent: datas.toAgent,
    toPayId: datas.toPayId,
    toSiteId: datas.toSiteId,
    toSiteName: datas.toSiteName,
    toCustId: datas.toCustId,
    toCurrencyId: datas.toCurrencyId,
    toSysId: datas.toSysId,
    toAccountNo: datas.toAccountNo,
    toAccountDisplayName: datas.toAccountDisplayName,
    toAmount: datas.toAmount,
    toFee: datas.toFee,

    reason: datas.reason,
    remark: datas.remark,
    createTime: datas.createTime,
    creatorName: datas.creatorName,
    creator: datas.creator,
    updateTime: datas.updateTime,
    updator: datas.updator,
    updatorName: datas.updatorName,

    setFromSiteList: [],
    setFromCurrencyList: [],
    setFromBankCodeList: [],

    setToSiteList: [],
    setToCurrencyList: [],
    setToBankCodeList: [],

    reasonList: [],
    payList: [],
    bankCodeList: [],
  };
};

export function reducer(state, action) {
  switch (action.type) {
    case defActionType.MAIN_SET_VALUE:
      return {
        ...state,
        [action.dataColumn]: action.value,
      };

    case defActionType.MAIN_SET_FROM_SITEID_VALUE:
      let getFromSiteId = null;
      let fromSiteName = null;
      if (action.value == 0) {
        getFromSiteId = state.setFromSiteList.find((x) => x.siteId == 1);
        fromSiteName = "Soon88";
      } else {
        getFromSiteId = state.setFromSiteList.find(
          (x) => x.siteId == action.value
        );
        fromSiteName = getFromSiteId.siteName;
      }

      let fromCurrencyList = [];

      fromCurrencyList.push({
        value: 0,
        displayText: "- Select Currency -",
      });

      getFromSiteId.iCurrencyList.map((i) => {
        fromCurrencyList.push({
          value: Number(i),
          displayText: window.i18n[`lbl_Currency${i}`],
        });
      });

      let initialFromBankList = [];
      initialFromBankList.push({
        value: 0,
        displayText: "- Select Bank Code -",
      });

      return {
        ...state,
        [action.dataColumn]: action.value,
        fromSiteName: fromSiteName,
        fromCurrencyId: 0,
        fromSysId: 0,
        fromAccountNo: "",
        fromAccountDisplayName: "",
        setFromCurrencyList: fromCurrencyList,
        setFromBankCodeList: initialFromBankList,
      };

    case defActionType.MAIN_SET_FROM_CURRENCY_VALUE:
      let fromBankList = [];
      fromBankList.push({
        value: 0,
        displayText: "- Select Bank Code -",
      });
      state.bankCodeList.map((s) => {
        if (s.currencyId == action.value) {
          fromBankList.push({
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
        setFromBankCodeList: fromBankList,
      };

    case defActionType.MAIN_SET_FROM_CHANNEL_VALUE:
      if (action.value == 3) {
        return {
          ...state,
          [action.dataColumn]: action.value,
          fromAgent: "",
        };
      } else {
        return {
          ...state,
          [action.dataColumn]: action.value,
          fromPayId: 0,
        };
      }

    case defActionType.MAIN_SET_TO_SITEID_VALUE:
      let getToSiteId = null;
      let toSiteName = null;
      if (action.value == 0) {
        getToSiteId = state.setToSiteList.find((x) => x.siteId == 1);
        toSiteName = "Soon88";
      } else {
        getToSiteId = state.setToSiteList.find((x) => x.siteId == action.value);
        toSiteName = getToSiteId.siteName;
      }

      let toCurrencyList = [];

      toCurrencyList.push({
        value: 0,
        displayText: "- Select Currency -",
      });

      getToSiteId.iCurrencyList.map((i) => {
        toCurrencyList.push({
          value: Number(i),
          displayText: window.i18n[`lbl_Currency${i}`],
        });
      });

      let initialToBankList = [];
      initialToBankList.push({
        value: 0,
        displayText: "- Select Bank Code -",
      });

      return {
        ...state,
        [action.dataColumn]: action.value,
        toSiteName: toSiteName,
        toCurrencyId: 0,
        toSysId: 0,
        toAccountNo: "",
        toAccountDisplayName: "",
        setToCurrencyList: toCurrencyList,
        setToBankCodeList: initialToBankList,
      };

    case defActionType.MAIN_SET_TO_CURRENCY_VALUE:
      let toBankList = [];
      toBankList.push({
        value: 0,
        displayText: "- Select Bank Code -",
      });
      state.bankCodeList.map((s) => {
        if (s.currencyId == action.value) {
          toBankList.push({
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
        setToBankCodeList: toBankList,
      };

    case defActionType.MAIN_SET_TO_CHANNEL_VALUE:
      if (action.value == 3) {
        return {
          ...state,
          [action.dataColumn]: action.value,
          toAgent: "",
        };
      } else {
        return {
          ...state,
          [action.dataColumn]: action.value,
          toPayId: 0,
        };
      }

    case defActionType.SITE_SET_LIST:
      return {
        ...state,
        setFromSiteList: action.siteList,
        setToSiteList: action.siteList,
      };

    case defActionType.FROM_CURRENCY_SET_LIST:
      console.log(action);
      let getFromSiteItem = null;
      let iniFromSiteName = null;
      if (action.fromSiteId == 0) {
        getFromSiteItem = action.siteList.find((x) => x.siteId == 1);
        iniFromSiteName = "Soon88";
      } else {
        getFromSiteItem = action.siteList.find(
          (x) => x.siteId == action.fromSiteId
        );
        console.log(getFromSiteItem);
        iniFromSiteName = getFromSiteItem.siteName;
      }

      let iniFromCurrencyList = [];

      iniFromCurrencyList.push({
        value: 0,
        displayText: "- Select Currency -",
      });

      getFromSiteItem.iCurrencyList.map((i) => {
        iniFromCurrencyList.push({
          value: Number(i),
          displayText: window.i18n[`lbl_Currency${i}`],
        });
      });

      return {
        ...state,
        fromSiteName: iniFromSiteName,
        setFromCurrencyList: iniFromCurrencyList,
      };

    case defActionType.TO_CURRENCY_SET_LIST:
      let getToSiteItem = null;
      let iniToSiteName = null;
      if (action.toSiteId == 0) {
        getToSiteItem = action.siteList.find((x) => x.siteId == 1);
        iniToSiteName = "Soon88";
      } else {
        getToSiteItem = action.siteList.find(
          (x) => x.siteId == action.toSiteId
        );
        iniToSiteName = getToSiteItem.siteName;
      }

      let iniToCurrencyList = [];

      iniToCurrencyList.push({
        value: 0,
        displayText: "- Select Currency -",
      });

      getToSiteItem.iCurrencyList.map((i) => {
        iniToCurrencyList.push({
          value: Number(i),
          displayText: window.i18n[`lbl_Currency${i}`],
        });
      });

      return {
        ...state,
        toSiteName: iniToSiteName,
        setToCurrencyList: iniToCurrencyList,
      };

    case defActionType.FROM_BANK_SET_LIST:
      let iniFromBankList = [];
      iniFromBankList.push({
        value: 0,
        displayText: "- Select Bank Code -",
      });
      state.bankCodeList.map((s) => {
        if (s.currencyId == action.fromCurrencyId) {
          iniFromBankList.push({
            value: s.value,
            currencyId: s.currencyId,
            displayText: s.displayText,
          });
        }
      });
      return {
        ...state,
        setFromBankCodeList: iniFromBankList,
      };

    case defActionType.TO_BANK_SET_LIST:
      let iniToBankList = [];
      iniToBankList.push({
        value: 0,
        displayText: "- Select Bank Code -",
      });
      state.bankCodeList.map((s) => {
        if (s.currencyId == action.toCurrencyId) {
          iniToBankList.push({
            value: s.value,
            currencyId: s.currencyId,
            displayText: s.displayText,
          });
        }
      });
      return {
        ...state,
        setToBankCodeList: iniToBankList,
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
