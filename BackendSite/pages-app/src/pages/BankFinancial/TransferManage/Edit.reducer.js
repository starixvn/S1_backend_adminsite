import { data } from "jquery";

export const defActionType = {
  MAIN_SET_VALUE: "MAIN_SET_VALUE",

  MAIN_SET_TRANSFERTYPE_VALUE: "MAIN_SET_TRANSFERTYPE_VALUE",

  MAIN_SET_FROM_CHANNEL_VALUE: "MAIN_SET_FROM_CHANNEL_VALUE",
  MAIN_SET_FROM_SITEID_VALUE: "MAIN_SET_FROM_SITEID_VALUE",
  MAIN_SET_FROM_CURRENCY_VALUE: "MAIN_SET_FROM_CURRENCY_VALUE",
  MAIN_SET_FROM_BANK_VALUE: "MAIN_SET_FROM_BANK_VALUE",
  MAIN_SET_FROM_ACCOUNT_VALUE: "MAIN_SET_FROM_ACCOUNT_VALUE",

  MAIN_SET_TO_CHANNEL_VALUE: "MAIN_SET_TO_CHANNEL_VALUE",
  MAIN_SET_TO_SITEID_VALUE: "MAIN_SET_TO_SITEID_VALUE",
  MAIN_SET_TO_CURRENCY_VALUE: "MAIN_SET_TO_CURRENCY_VALUE",
  MAIN_SET_TO_BANK_VALUE: "MAIN_SET_TO_BANK_VALUE",
  MAIN_SET_TO_ACCOUNT_VALUE: "MAIN_SET_TO_ACCOUNT_VALUE",

  //Initial List
  SITE_SET_LIST: "SITE_SET_LIST",
  FROM_CURRENCY_SET_LIST: "FROM_CURRENCY_SET_LIST",
  TO_CURRENCY_SET_LIST: "TO_CURRENCY_SET_LIST",
  FROM_BANK_SET_LIST: "FROM_BANK_SET_LIST",
  TO_BANK_SET_LIST: "TO_BANK_SET_LIST",
  FROM_ACCOUNT_SET_LIST: "FROM_ACCOUNT_SET_LIST",
  TO_ACCOUNT_SET_LIST: "TO_ACCOUNT_SET_LIST",
  CHANNEL_SET_VALUE: "CHANNEL_SET_VALUE",
  PAY_SET_LIST: "PAY_SET_LIST",
  BANK_CODE_SET_LIST: "BANK_CODE_SET_LIST",
  REASON_SET_LIST: "REASON_SET_LIST",
  TRANSFER_ACCOUNT_SET_LIST: "TRANSFER_ACCOUNT_SET_LIST",

  //Upload Receipt
  SET_UPLOAD_RECEIPT: "SET_UPLOAD_RECEIPT",
};

export const initialState = (datas, isAdd) => {
  if (isAdd) {
  }

  return {
    transferId: datas.transferId,
    transferType: datas.transferType,
    transactionType: datas.transactionType,
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
    fromAccount: datas.fromAccountNo,
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
    toAccount: datas.toAccountNo,
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

    uploadReceipt: datas.uploadReceipt,
    uploadReceiptName: datas.uploadReceiptName,
    transferPic: datas.transferPic,
    setFromSiteList: [],
    setFromCurrencyList: [],
    setFromBankCodeList: [],
    setFromAccountList: [],

    setToSiteList: [],
    setToCurrencyList: [],
    setToBankCodeList: [],
    setToAccountList: [],

    reasonList: [],
    payList: [],
    bankCodeList: [],
    transferAccountList: [],
  };
};

export function reducer(state, action) {
  switch (action.type) {
    case defActionType.MAIN_SET_VALUE:
      return {
        ...state,
        [action.dataColumn]: action.value,
      };

    case defActionType.MAIN_SET_TRANSFERTYPE_VALUE:
      let defaultTransactionType = 0;
      if (action.value != 4) {
        defaultTransactionType = 3;
      }
      return {
        ...state,
        [action.dataColumn]: action.value,
        transactionType: defaultTransactionType,
      };

    case defActionType.MAIN_SET_FROM_SITEID_VALUE:
      let getFromSiteId = null;
      let fromSiteName = null;
      let fromCurrencyList = [];
      let initialFromBankList = [];

      fromCurrencyList.push({
        value: 0,
        displayText: "- Select Currency -",
      });

      initialFromBankList.push({
        value: 0,
        displayText: "- Select Bank Code -",
      });

      getFromSiteId = state.setFromSiteList.find(
        (x) => x.value == action.value
      );

      if (getFromSiteId) {
        fromSiteName = getFromSiteId.displayText;

        if (getFromSiteId.iCurrencyList) {
          getFromSiteId.iCurrencyList.map((i) => {
            fromCurrencyList.push({
              value: Number(i),
              displayText: window.i18n[`lbl_Currency${i}`],
            });
          });
        }
        if (action.value == 1) {
          if (!getFromSiteId.iCurrencyList.find((x) => x == 13)) {
            fromCurrencyList.push({
              value: Number(13),
              displayText: window.i18n[`lbl_Currency${13}`],
            });
          }
        }
      }

      return {
        ...state,
        [action.dataColumn]: action.value,
        fromSiteName: fromSiteName,
        fromCurrencyId: 0,
        fromSysId: 0,
        fromAgnet: "",
        fromPayId: "",
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
        fromSysId: 0,
        fromAgent: "",
        fromPayId: 0,
        setFromBankCodeList: fromBankList,
      };

    case defActionType.MAIN_SET_FROM_CHANNEL_VALUE:
      let refreshFromAccountList = [];
      refreshFromAccountList.push({
        value: 0,
        displayText: "- Select Account -",
      });
      state.transferAccountList.map((s) => {
        if (
          s.channel == Number(action.value) &&
          s.siteId == Number(state.fromSiteId) &&
          s.currencyId == Number(state.fromCurrencyId) &&
          s.sysId == Number(state.fromSysId)
        ) {
          refreshFromAccountList.push({
            value: s.value,
            displayText: s.displayText,
          });
        }
      });
      if (action.value == 3) {
        return {
          ...state,
          [action.dataColumn]: action.value,
          fromAgent: "",
          fromPayId: 0,
          setFromAccountList: refreshFromAccountList,
          fromAccount: 0,
          fromAccountNo: "",
          fromAccountDisplayName: "",
        };
      } else {
        return {
          ...state,
          [action.dataColumn]: action.value,
          fromPayId: 0,
          fromAgent: "",
          fromAccount: 0,
          setFromAccountList: refreshFromAccountList,
          fromAccountNo: "",
          fromAccountDisplayName: "",
        };
      }

    case defActionType.MAIN_SET_FROM_BANK_VALUE:
      let fromAccountList = [];
      fromAccountList.push({
        value: 0,
        displayText: "- Select Account -",
      });
      state.transferAccountList.map((s) => {
        if (
          s.channel == Number(state.fromChannel) &&
          s.siteId == Number(state.fromSiteId) &&
          s.currencyId == Number(state.fromCurrencyId) &&
          s.sysId == Number(action.value)
        ) {
          fromAccountList.push({
            value: s.value,
            displayText: s.displayText,
          });
        }
      });
      return {
        ...state,
        [action.dataColumn]: action.value,
        setFromAccountList: fromAccountList,
        fromAccount: 0,
        fromAgent: "",
        fromPayId: 0,
        fromAccountNo: "",
        fromAccountDisplayName: "",
      };

    case defActionType.MAIN_SET_FROM_ACCOUNT_VALUE:
      let getFromAccountItem = state.transferAccountList.find(
        (x) => x.value == action.value
      );
      return {
        ...state,
        fromAccount: getFromAccountItem.value,
        fromAccountNo: getFromAccountItem.value,
        fromAgent: getFromAccountItem.agent,
        fromPayId: getFromAccountItem.payId,
        fromAccountDisplayName: getFromAccountItem.displayText,
      };

    case defActionType.MAIN_SET_TO_SITEID_VALUE:
      let getToSiteId = null;
      let toSiteName = null;
      let toCurrencyList = [];
      let initialToBankList = [];

      toCurrencyList.push({
        value: 0,
        displayText: "- Select Currency -",
      });

      initialToBankList.push({
        value: 0,
        displayText: "- Select Bank Code -",
      });

      getToSiteId = state.setToSiteList.find((x) => x.value == action.value);

      if (getToSiteId) {
        toSiteName = getToSiteId.displayText;

        if (getToSiteId.iCurrencyList) {
          getToSiteId.iCurrencyList.map((i) => {
            toCurrencyList.push({
              value: Number(i),
              displayText: window.i18n[`lbl_Currency${i}`],
            });
          });
        }
        if (action.value == 1) {
          if (!getToSiteId.iCurrencyList.find((x) => x == 13)) {
            toCurrencyList.push({
              value: Number(13),
              displayText: window.i18n[`lbl_Currency${13}`],
            });
          }
        }
      }

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
        toSysId: 0,
        toAgent: "",
        toPayId: 0,
        setToBankCodeList: toBankList,
      };

    case defActionType.MAIN_SET_TO_CHANNEL_VALUE:
      let refreshToAccountList = [];
      refreshToAccountList.push({
        value: 0,
        displayText: "- Select Account -",
      });
      state.transferAccountList.map((s) => {
        if (
          s.channel == Number(action.value) &&
          s.siteId == Number(state.toSiteId) &&
          s.currencyId == Number(state.toCurrencyId) &&
          s.sysId == Number(state.toSysId)
        ) {
          refreshToAccountList.push({
            value: s.value,
            displayText: s.displayText,
          });
        }
      });

      if (action.value == 3) {
        return {
          ...state,
          [action.dataColumn]: action.value,
          toAgent: "",
          toPayId: 0,
          setToAccountList: refreshToAccountList,
          toAccount: 0,
          toAccountNo: "",
          toAccountDisplayName: "",
        };
      } else {
        return {
          ...state,
          [action.dataColumn]: action.value,
          toAgent: "",
          toPayId: 0,
          setToAccountList: refreshToAccountList,
          toAccount: 0,
          toAccountNo: "",
          toAccountDisplayName: "",
        };
      }

    case defActionType.MAIN_SET_TO_BANK_VALUE:
      let toAccountList = [];
      toAccountList.push({
        value: 0,
        displayText: "- Select Account -",
      });
      state.transferAccountList.map((s) => {
        if (
          s.channel == Number(state.toChannel) &&
          s.siteId == Number(state.toSiteId) &&
          s.currencyId == Number(state.toCurrencyId) &&
          s.sysId == Number(action.value)
        ) {
          toAccountList.push({
            value: s.value,
            displayText: s.displayText,
          });
        }
      });
      return {
        ...state,
        [action.dataColumn]: action.value,
        setToAccountList: toAccountList,
        toAccount: 0,
        toAccountNo: "",
        toAccountDisplayName: "",
      };

    case defActionType.MAIN_SET_TO_ACCOUNT_VALUE:
      let getToAccountItem = state.transferAccountList.find(
        (x) => x.value == action.value
      );
      return {
        ...state,
        toAccount: getToAccountItem.value,
        toAccountNo: getToAccountItem.value,
        toAgent: getToAccountItem.agent,
        toPayId: getToAccountItem.payId,
        toAccountDisplayName: getToAccountItem.displayText,
      };

    case defActionType.SITE_SET_LIST:
      var iniSiteList = [];

      iniSiteList.push({
        value: 0,
        displayText: "- Select Site -",
      });

      action.siteList.map((s) => {
        iniSiteList.push({
          value: s.siteId,
          displayText: s.siteName,
          iCurrencyList: s.iCurrencyList,
        });
      });

      return {
        ...state,
        setFromSiteList: iniSiteList,
        setToSiteList: iniSiteList,
      };

    case defActionType.FROM_CURRENCY_SET_LIST:
      let getFromSiteItem = null;
      let iniFromSiteName = null;
      let iniFromCurrencyList = [];

      iniFromCurrencyList.push({
        value: 0,
        displayText: "- Select Currency -",
      });

      getFromSiteItem = action.siteList.find(
        (x) => x.siteId == action.fromSiteId
      );

      if (getFromSiteItem) {
        iniFromSiteName = getFromSiteItem.siteName;

        getFromSiteItem.iCurrencyList.map((i) => {
          iniFromCurrencyList.push({
            value: Number(i),
            displayText: window.i18n[`lbl_Currency${i}`],
          });
        });

        if (action.fromSiteId == 1) {
          if (!getFromSiteItem.iCurrencyList.find((x) => x == 13)) {
            iniFromCurrencyList.push({
              value: Number(13),
              displayText: window.i18n[`lbl_Currency${13}`],
            });
          }
        }
      }

      return {
        ...state,
        fromSiteName: iniFromSiteName,
        setFromCurrencyList: iniFromCurrencyList,
      };

    case defActionType.TO_CURRENCY_SET_LIST:
      let getToSiteItem = null;
      let iniToSiteName = null;
      let iniToCurrencyList = [];

      iniToCurrencyList.push({
        value: 0,
        displayText: "- Select Currency -",
      });

      getToSiteItem = action.siteList.find((x) => x.siteId == action.toSiteId);

      if (getToSiteItem) {
        iniToSiteName = getToSiteItem.siteName;

        getToSiteItem.iCurrencyList.map((i) => {
          iniToCurrencyList.push({
            value: Number(i),
            displayText: window.i18n[`lbl_Currency${i}`],
          });
        });

        if (action.toSiteId == 1) {
          if (!getToSiteItem.iCurrencyList.find((x) => x == 13)) {
            iniToCurrencyList.push({
              value: Number(13),
              displayText: window.i18n[`lbl_Currency${13}`],
            });
          }
        }
      }

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

    case defActionType.FROM_ACCOUNT_SET_LIST:
      let iniFromAccountList = [];
      iniFromAccountList.push({
        value: 0,
        displayText: "- Select Account -",
      });
      state.transferAccountList.map((s) => {
        if (
          s.channel == Number(action.fromChannel) &&
          s.siteId == Number(action.fromSiteId) &&
          s.currencyId == Number(action.fromCurrencyId) &&
          s.sysId == Number(action.fromSysId)
        ) {
          iniFromAccountList.push({
            value: s.value,
            displayText: s.displayText,
          });
        }
      });
      return {
        ...state,
        setFromAccountList: iniFromAccountList,
      };

    case defActionType.TO_ACCOUNT_SET_LIST:
      console.log(action);
      console.log(state.transferAccountList);
      let iniToAccountList = [];
      iniToAccountList.push({
        value: 0,
        displayText: "- Select Account -",
      });
      state.transferAccountList.map((s) => {
        if (
          s.channel == Number(action.toChannel) &&
          s.siteId == Number(action.toSiteId) &&
          s.currencyId == Number(action.toCurrencyId) &&
          s.sysId == Number(action.toSysId)
        ) {
          iniToAccountList.push({
            value: s.value,
            displayText: s.displayText,
          });
        }
      });
      return {
        ...state,
        setToAccountList: iniToAccountList,
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
        payList: setPayList,
      };

    case defActionType.TRANSFER_ACCOUNT_SET_LIST:
      let setTransferAccountList = [];
      setTransferAccountList.push({
        value: 0,
        displayText: "- Select Account -",
      });
      action.transferAccountList.map((s) => {
        setTransferAccountList.push({
          value: s.accountNo,
          displayText: s.displayName,
          siteId: s.siteId,
          currencyId: s.currencyId,
          sysId: s.sysId,
          payId: s.payId,
          agent: s.agent,
          channel: s.channel,
          purpose: s.purpose,
        });
      });
      return {
        ...state,
        transferAccountList: setTransferAccountList,
      };

    case defActionType.SET_UPLOAD_RECEIPT:
      return {
        ...state,
        transferPic: action.uploadReceiptPic.data.fileBytes,
        uploadReceiptName: "",
      };

    default:
      throw new Error();
  }
}
