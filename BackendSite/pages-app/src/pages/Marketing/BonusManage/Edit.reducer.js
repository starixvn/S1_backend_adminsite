import moment from "moment";
export const defActionType = {
	CURRENCY_SET_VALUE: "CURRENCY_SET_VALUE",
	MAIN_SET_VALUE: "MAIN_SET_VALUE",
	MAIN_SET_CURRENCY_CHECKBOX: "MAIN_SET_CURRENCY_CHECKBOX",
	MAIN_SET_SITE: "MAIN_SET_SITE",
	CURRENCY_LANG_SET_VALUE: "CURRENCY_LANG_SET_VALUE"
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
			status: 2,
			siteId: siteList[0].siteId,
			selectedCurrencies: [],
			currentTab: null,
			specific: _specific
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
			selectedCurrencies: _selectedCurrencies,
			currentTab: _selectedCurrencies[0],
			specific: _specific
		};
	}
};

const initLang = {
	bannerUrl: "bannerUrl",
	title: "title",
	detail: "detail"
};

const initSpecific = {
	usage: "0",
	rollOver: "",
	bonusAmountType: 1,
	bonus: "",
	maxBonus: "",
	status: 2,
	selectedLang: [],
	currentLang: null,
	lang: {
		en: { ...initLang }
	}
};

//const jsonField = ["title", "bannerUrl", "detail"];

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
			let lang = JSON.parse(d.details);
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
			return {
				...state,
				specific: {
					...state.specific,
					[action.currencyId]: {
						...state.specific[action.currencyId],
						lang: {
							...state.specific[action.currencyId].lang,
							[action.lang]: {
								...state.specific[action.currencyId].lang[
									action.lang
								],
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
			let newCurrencies = state[action.dataColumn].includes(action.value)
				? state[action.dataColumn].filter(v => v !== action.value)
				: [...state[action.dataColumn], action.value];

			let currentTab = state.currentTab;
			if (newCurrencies.length === 0) {
				currentTab = null;
			} else if (currentTab === null) {
				currentTab = newCurrencies[0];
			}

			return {
				...state,
				[action.dataColumn]: newCurrencies,
				currentTab: currentTab
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

		default:
			throw new Error();
	}
}
