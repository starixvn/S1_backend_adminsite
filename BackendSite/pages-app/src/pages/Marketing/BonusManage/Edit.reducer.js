import moment from "moment";
export const defActionType = {
	CURRENCY_SET_VALUE: "CURRENCY_SET_VALUE",
	MAIN_SET_VALUE: "MAIN_SET_VALUE",
	MAIN_SET_CURRENCY_CHECKBOX: "MAIN_SET_CURRENCY_CHECKBOX",
	MAIN_SET_SITE: "MAIN_SET_SITE"
};

export const initialState = (IsAdd, datas, siteList) => {
	if (IsAdd) {
		let _specific = SetSpecific(siteList[0].currencyList, null);
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
		let _specific = SetSpecific(siteList[0].currencyList, datas.specific);
		return {
			bonusCode: datas.bonusCode,
			promotionType: datas.promotionType,
			periodFrom: datas.periodFrom,
			periodTo: datas.periodTo,
			expiryDays: datas.expiryDays,
			description: datas.description,
			status: datas.status,
			siteId: datas.siteId,
			selectedCurrencies: Object.keys(datas.specific).map(c => {
				return c;
			}),
			currentTab: null,
			specific: _specific
		};
	}
};

const initSpecific = {
	usage: "0",
	rollOver: "",
	bonusAmountType: 1,
	bonus: "",
	maxBonus: "",
	bannerUrl: "",
	title: "",
	detail: "",
	status: 2
};

const SetSpecific = (currencyList, data) => {
	let _specific = {};
	currencyList.forEach(currency => {
		_specific[currency] = { ...initSpecific };
	});

	if (data !== null) {
		data.forEach(d => {
			let _newData = {};
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

		case defActionType.MAIN_SET_VALUE:
			return {
				...state,
				[action.dataColumn]: action.value
			};

		case defActionType.MAIN_SET_CURRENCY_CHECKBOX:
			return state[action.dataColumn].includes(action.value)
				? {
						...state,
						[action.dataColumn]: state[action.dataColumn].filter(
							v => v !== action.value
						)
				  }
				: {
						...state,
						[action.dataColumn]: [
							...state[action.dataColumn],
							action.value
						]
				  };
		case defActionType.MAIN_SET_SITE:
			return {
				...state,
				siteId: action.siteId,
				currentTab: null,
				selectedCurrencies: [],
				specific: SetSpecific(action.currencyList, null)
			};

		default:
			throw new Error();
	}
}
