import moment from "moment";
export const defActionType = {
	CURRENCY_SET_VALUE: "CURRENCY_SET_VALUE",
	MAIN_SET_VALUE: "MAIN_SET_VALUE",
	MAIN_SET_CURRENCY_CHECKBOX: "MAIN_SET_CURRENCY_CHECKBOX",
	MAIN_SET_SITE: "MAIN_SET_SITE",
	CURRENCY_LANG_SET_VALUE: "CURRENCY_LANG_SET_VALUE",
	CURRENCY_SET_LANG_CHECKBOX: "CURRENCY_SET_LANG_CHECKBOX",
	GET_PROMO_CATEGORIES: "GET_PROMO_CATEGORIES",
	SET_TPID: "SET_TPID"
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
			categoryId: null
		};
	} else {
		let _specific = SetSpecific(
			siteList[0].currencyList,
			siteList[0].langList,
			datas.specific
		);
		let _selectedCurrencies = datas.specific.map(s => {
			return s.currencyId;
		});
		return {
			bonusCode: datas.bonusCode,
			promotionType: datas.promotionType,
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
			categoryId: datas.categoryId
		};
	}
};

const initLang = {
	bannerUrl: "",
	description: "",
	title: "",
	detail: ""
};

const initSpecific = {
	usage: "0",
	rollOver: "",
	bonusAmountType: null,
	bonus: "",
	maxBonus: "",
	status: 2,
	selectedLang: [],
	currentLang: null,
	lang: {
		en: { ...initLang }
	}
};

const SetSpecific = (currencyList, langList, data) => {
	let _specific = {};
	currencyList.forEach(currency => {
		_specific[currency] = { ...initSpecific };
		let lang = {};
		langList.forEach(l => {
			lang = {
				...lang,
				[l]: { ...initLang }
			};
		});
		_specific[currency].lang = lang;
	});

	if (data !== null) {
		data.forEach(d => {
			let _newData = {};
			let lang = JSON.parse(d.details ?? "{}");
			d["lang"] = lang;
			d["selectedLang"] = Object.keys(lang);
			langList
				.filter(l => {
					return !d["selectedLang"].includes(l);
				})
				.forEach(l => {
					d.lang = {
						...d.lang,
						[l]: { ...initLang }
					};
				});
			d["currentLang"] = d["selectedLang"][0] ?? "";
			Object.keys(initSpecific).forEach(k => {
				if (k === "bonusAmountType") {
					_newData[k] = d.bonusType;
				} else {
					_newData[k] = d[k] ?? "";
				}
			});
			_specific[d.currencyId] = _newData;
		});
	}

	return _specific;
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
						[action.dataColumn]: action.value
					}
				}
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
								[action.dataColumn]: action.value
							}
						}
					}
				}
			};

		case defActionType.MAIN_SET_VALUE:
			return {
				...state,
				[action.dataColumn]: action.value
			};

		case defActionType.MAIN_SET_CURRENCY_CHECKBOX:
			let isRemove = state[action.dataColumn].includes(action.value);
			let newCurrencies = isRemove
				? state[action.dataColumn].filter(v => v !== action.value)
				: [...state[action.dataColumn], action.value];

			let currentTab = state.currentTab;
			if (newCurrencies.length === 0) {
				currentTab = null;
			} else if (currentTab === null) {
				currentTab = newCurrencies[0];
			}

			if (isRemove && !newCurrencies.includes(currentTab)) {
				currentTab =
					newCurrencies.length === 0 ? null : newCurrencies[0];
			}

			return {
				...state,
				[action.dataColumn]: newCurrencies,
				currentTab: currentTab
			};

		case defActionType.CURRENCY_SET_LANG_CHECKBOX:
			let selCurr = state.currentTab;
			let selLangs = state.specific[selCurr].selectedLang;
			let isLangRemove = selLangs.includes(action.value);
			let newLangs = isLangRemove
				? selLangs.filter(v => v !== action.value)
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
						currentLang: currLang
					}
				}
			};

		case defActionType.MAIN_SET_SITE:
			return {
				...state,
				siteId: action.siteId,
				currentTab: null,
				selectedCurrencies: [],
				specific: SetSpecific(
					action.currencyList,
					action.langList,
					null
				)
			};

		case defActionType.GET_PROMO_CATEGORIES:
			let arr = action.productCategory.map(item => {
				return {
					value: item.id,
					displayText: item.name,
					tpId: item.tpId
				};
			});
			let _value =
				action.value !== undefined ? action.value : state.walletType;

			let tpId = action.isInit ? state.tpId : null;
			let categoryId = action.isInit ? state.categoryId : null;
			return {
				...state,
				walletType: _value,
				productCategory: arr,
				tpId: tpId,
				categoryId: categoryId
			};

		case defActionType.SET_TPID:
			let categoryInfo = state.productCategory[action.idx];
			let _tpId = categoryInfo.tpId;
			let _categoryId = categoryInfo.value;

			return {
				...state,
				tpId: _tpId,
				categoryId: _categoryId
			};

		default:
			throw new Error();
	}
}
