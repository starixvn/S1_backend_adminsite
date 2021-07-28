import moment from "moment";
export const defActionType = {
  CURRENCY_SET_VALUE: "CURRENCY_SET_VALUE",
  MAIN_SET_VALUE: "MAIN_SET_VALUE",
  MAIN_SET_CURRENCY_CHECKBOX: "MAIN_SET_CURRENCY_CHECKBOX",
  MAIN_SET_SITE: "MAIN_SET_SITE",
  CURRENCY_LANG_SET_VALUE: "CURRENCY_LANG_SET_VALUE",
  CURRENCY_SET_LANG_CHECKBOX: "CURRENCY_SET_LANG_CHECKBOX",
  GET_PROMO_CATEGORIES: "GET_PROMO_CATEGORIES",
  SET_TPID: "SET_TPID",
  CURRENCY_SET_AMOUNT_VALUE: "CURRENCY_SET_AMOUNT_VALUE",
  GET_TPGAME_CATEGORIES: "GET_TPGAME_CATEGORIES",
  CURRENCY_SET_TpGame_VALUE: "CURRENCY_SET_TpGame_VALUE",
  SPECIALBONUS_VALUE: "SPECIALBONUS_VALUE",
  ONLY_REGISTERED_SET_VALUE: "ONLY_REGISTERED_SET_VALUE",
  AUTOEXTEND_SET_VALUE: "AUTOEXTEND_SET_VALUE",
  GET_SITE_VIPLEVEL: "GET_SITE_VIPLEVEL",
  FORVIPLEVEL_SET_VALUE: "FORVIPLEVEL_SET_VALUE",
  VIPLEVEL_SET_VALUE: "VIPLEVEL_SET_VALUE",
};

export const initialState = (IsAdd, datas, siteList) => {
  if (IsAdd) {
    let _specific = SetSpecific(
      siteList[0].currencyList,
      siteList[0].langList,
      null
    );
    return {
      bonusCode: "",
      promotionType: 1,
      promotionAction: null,
      promotionDeviceType: 1,
      periodFrom: moment(),
      periodTo: moment(),
      expiryDays: "",
      description: "",
      walletType: null,
      tpId: null,
      status: 2,
      siteId: siteList[0].siteId,
      selectedCurrencies: [],
      currentTab: null,
      sort: "",
      specific: _specific,
      productCategory: [],
      categoryId: null,
      periodType: null,
      periodWeek: 1,
      periodMonth: 1,
      redeemHours: "",
      tpGameCategory: [],
      specialCode: "",
      isSpecial: false,
      specialBonusCheck: [],
      onlyRegistered: [],
      autoExtend: [],
      allSiteVipLevelList: [], //All site vip leve info.
      vipLevelList: [], //Initial Vip Level SelectedItem.
      forVipLevel: [], //Use VipLevel Or not
      selectedVipLevel: [], //If User VipLevel,Which Vip if fit.
    };
  } else {
    let site = siteList.find((s) => s.siteId === datas.siteId);
    let _specific = SetSpecific(
      site.currencyList,
      site.langList,
      datas.specific
    );
    let _selectedCurrencies = datas.specific.map((s) => {
      return s.currencyId;
    });

    //promotion display condiction
    let _forVipLevel = [];
    let _selcectedVipLevel = [];
    if (datas.promotionDisplayCondition.length > 0) {
      let _setVipLevel = SetVipLevel(datas.promotionDisplayCondition);
      _forVipLevel = _setVipLevel.isForVipLevel;
      _selcectedVipLevel = _setVipLevel.selectedVipLevel;
    }
    return {
      bonusCode: datas.bonusCode,
      promotionType: datas.promotionType,
      promotionAction: datas.promotionAction,
      promotionDeviceType: datas.promotionDeviceType,
      periodFrom: datas.periodFrom,
      periodTo: datas.periodTo,
      expiryDays: datas.expiryDays,
      description: datas.description,
      status: datas.status,
      siteId: datas.siteId,
      sort: datas.sort,
      selectedCurrencies: _selectedCurrencies,
      currentTab: _selectedCurrencies[0],
      specific: _specific,
      walletType: datas.walletType,
      tpId: datas.tpId,
      productCategory: datas.productCategory,
      categoryId: datas.categoryId,
      periodType: datas.periodType,
      periodWeek: datas.periodWeek ? datas.periodWeek : 1,
      periodMonth: datas.periodMonth ? datas.periodMonth : 1,
      redeemHours: datas.redeemHours,
      tpGameCategory: [],
      specialCode: datas.specialCode,
      isSpecial: datas.isSpecial,
      specialBonusCheck: datas.isSpecial ? [1] : [],
      onlyRegistered: datas.onlyRegistered ? [1] : [],
      autoExtend: datas.autoExtend ? [1] : [],
      allSiteVipLevelList: [],
      vipLevelList: [],
      forVipLevel: _forVipLevel,
      selectedVipLevel: _selcectedVipLevel,
    };
  }
};

const initLang = {
  bannerUrl: "",
  description: "",
  title: "",
  detail: "",
};

const initSpecific = {
  affid: "",
  usage: "0",
  rollOver: "",
  totalUsageLimit: "0",
  bonusAmountType: null,
  bonus: "",
  maxBonus: "",
  status: 2,
  selectedLang: [],
  currentLang: null,
  requiredAmount: "",
  lockedType: 1,
  beenUsed: 0,
  lang: {
    en: { ...initLang },
  },
  tpGames: {},
  minPayout: "",
};

const SetSpecific = (currencyList, langList, data) => {
  let _specific = {};
  currencyList.forEach((currency) => {
    _specific[currency] = { ...initSpecific };
    let lang = {};
    langList.forEach((l) => {
      lang = {
        ...lang,
        [l]: { ...initLang },
      };
    });
    _specific[currency].lang = lang;
  });

  if (data !== null) {
    data.forEach((d) => {
      let _newData = {};
      let lang = JSON.parse(d.details ?? "{}");
      let _tpGameData = {};
      d["lang"] = lang;
      d["selectedLang"] = Object.keys(lang);
      langList
        .filter((l) => {
          return !d["selectedLang"].includes(l);
        })
        .forEach((l) => {
          d.lang = {
            ...d.lang,
            [l]: { ...initLang },
          };
        });
      d["currentLang"] = d["selectedLang"][0] ?? "";
      Object.keys(initSpecific).forEach((k) => {
        if (k === "bonusAmountType") {
          _newData[k] = d.bonusType;
        } else if (k === "affid") {
          _newData[k] = d.vN_AffCodes;
        } else {
          _newData[k] = d[k] ?? "";
        }
      });

      _specific[d.currencyId] = _newData;

      if (d.rebateList != null) {
        _specific[d.currencyId].tpGames = d.rebateList.forEach((r) => {
          _tpGameData[r.gameType] = {
            percentage: r.percentage,
            status: r.status,
          };
        });
        _specific[d.currencyId].tpGames = _tpGameData;
      }
    });
  }
  return _specific;
};

const SetVipLevel = (data) => {
  let _isForVipLevel = [];
  let _vipLevel = {};
  data.map((s) => {
    if (s.conditionType == 1) {
      _isForVipLevel = [1];
      _vipLevel = s.description.vipLevel;
    }
  });
  return { isForVipLevel: _isForVipLevel, selectedVipLevel: _vipLevel };
};

export function reducer(state, action) {
  switch (action.type) {
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

    case defActionType.CURRENCY_LANG_SET_VALUE:
      let selCurrency = state.currentTab;
      let selLang = state.specific[selCurrency].currentLang;
      return {
        ...state,
        specific: {
          ...state.specific,
          [selCurrency]: {
            ...state.specific[selCurrency],
            lang: {
              ...state.specific[selCurrency].lang,
              [selLang]: {
                ...state.specific[selCurrency].lang[selLang],
                [action.dataColumn]: action.value,
              },
            },
          },
        },
      };

    case defActionType.MAIN_SET_VALUE:
      return {
        ...state,
        [action.dataColumn]: action.value,
      };

    case defActionType.CURRENCY_SET_AMOUNT_VALUE:
      return {
        ...state,
        specific: {
          ...state.specific,
          [action.currencyId]: {
            ...state.specific[action.currencyId],
            [action.dataColumn]: action.value,
            maxBonus: action.value === 2 ? 0 : "",
          },
        },
      };

    case defActionType.MAIN_SET_CURRENCY_CHECKBOX:
      let isRemove = state[action.dataColumn].includes(action.value);
      let newCurrencies = isRemove
        ? state[action.dataColumn].filter((v) => v !== action.value)
        : [...state[action.dataColumn], action.value];

      let currentTab = state.currentTab;
      if (newCurrencies.length === 0) {
        currentTab = null;
      } else if (currentTab === null) {
        currentTab = newCurrencies[0];
      }

      if (isRemove && !newCurrencies.includes(currentTab)) {
        currentTab = newCurrencies.length === 0 ? null : newCurrencies[0];
      }

      return {
        ...state,
        [action.dataColumn]: newCurrencies,
        currentTab: currentTab,
      };

    case defActionType.CURRENCY_SET_LANG_CHECKBOX:
      let selCurr = state.currentTab;
      let selLangs = state.specific[selCurr].selectedLang;
      let isLangRemove = selLangs.includes(action.value);
      let newLangs = isLangRemove
        ? selLangs.filter((v) => v !== action.value)
        : [...selLangs, action.value];

      let currLang = state.specific[selCurr].currentLang;
      if (newLangs.length === 0) {
        currLang = null;
      } else if (currLang === null || currLang === "") {
        currLang = newLangs[0];
      }

      if (isLangRemove && !newLangs.includes(currLang)) {
        currLang = newLangs.length === 0 ? null : newLangs[0];
      }

      return {
        ...state,
        specific: {
          ...state.specific,
          [selCurr]: {
            ...state.specific[selCurr],
            selectedLang: newLangs,
            currentLang: currLang,
          },
        },
      };

    case defActionType.MAIN_SET_SITE:
      let newInitialVipLevelList = [];

      state.allSiteVipLevelList.map((s) => {
        if (s.siteId == action.siteId) {
          newInitialVipLevelList.push({
            value: s.level,
            displayText: s.level + "-" + s.levelName,
          });
        }
      });

      return {
        ...state,
        siteId: action.siteId,
        currentTab: null,
        selectedCurrencies: [],
        specific: SetSpecific(action.currencyList, action.langList, null, null),
        forVipLevel: [],
        vipLevelList: newInitialVipLevelList,
        selectedVipLevel: [],
      };

    case defActionType.GET_PROMO_CATEGORIES:
      let arr = action.productCategory.map((item) => {
        return {
          value: item.id,
          displayText: item.name,
          tpId: item.tpId,
        };
      });
      let _value = action.value !== undefined ? action.value : state.walletType;

      let tpId = action.isInit ? state.tpId : null;
      let categoryId = action.isInit ? state.categoryId : null;
      return {
        ...state,
        walletType: _value,
        productCategory: arr,
        tpId: tpId,
        categoryId: categoryId,
      };

    case defActionType.SET_TPID:
      let categoryInfo = state.productCategory[action.idx];
      let _tpId = categoryInfo.tpId;
      let _categoryId = categoryInfo.value;

      return {
        ...state,
        tpId: _tpId,
        categoryId: _categoryId,
      };

    case defActionType.GET_TPGAME_CATEGORIES:
      let tpGameCategory = action.tpGameCategory.map((item) => {
        return {
          value: item.type,
          displayText: item.typeName,
        };
      });
      return {
        ...state,
        tpGameCategory: tpGameCategory,
      };

    case defActionType.CURRENCY_SET_TpGame_VALUE:
      return {
        ...state,
        specific: {
          ...state.specific,
          [action.currencyId]: {
            ...state.specific[action.currencyId],
            tpGames: {
              ...state.specific[action.currencyId].tpGames,
              [action.gameType]: {
                ...state.specific[action.currencyId].tpGames[action.gameType],
                [action.dataColumn]: action.value,
                gameName: action.gameName,
              },
            },
          },
        },
      };

    case defActionType.SPECIALBONUS_VALUE:
      return {
        ...state,
        specialBonusCheck: action.specialBonusCheck,
        isSpecial: action.isSpecial,
      };

    case defActionType.ONLY_REGISTERED_SET_VALUE:
      let newValue = state.onlyRegistered.includes(action.value)
        ? []
        : [action.value];
      return {
        ...state,
        onlyRegistered: newValue,
      };

    case defActionType.AUTOEXTEND_SET_VALUE:
      let newAutoValue = state.autoExtend.includes(action.value)
        ? []
        : [action.value];
      return {
        ...state,
        autoExtend: newAutoValue,
      };

    case defActionType.GET_SITE_VIPLEVEL:
      let initialVipLevelList = [];

      action.allSiteVipLevelList.map((s) => {
        if (s.siteId == state.siteId) {
          initialVipLevelList.push({
            value: s.level,
            displayText: s.level + "-" + s.levelName,
          });
        }
      });
      return {
        ...state,
        allSiteVipLevelList: action.allSiteVipLevelList,
        vipLevelList: initialVipLevelList,
      };

    case defActionType.FORVIPLEVEL_SET_VALUE:
      let newForVipLevelValue = state.forVipLevel.includes(action.value)
        ? []
        : [action.value];
      return {
        ...state,
        forVipLevel: newForVipLevelValue,
        selectedVipLevel: [],
      };

    case defActionType.VIPLEVEL_SET_VALUE:
      let isVipLevelRemove = state[action.dataColumn].includes(action.value);
      let newSelectedVipLevel = isVipLevelRemove
        ? state[action.dataColumn].filter((v) => v !== action.value)
        : [...state[action.dataColumn], action.value];

      return {
        ...state,
        [action.dataColumn]: newSelectedVipLevel,
      };

    default:
      throw new Error();
  }
}
