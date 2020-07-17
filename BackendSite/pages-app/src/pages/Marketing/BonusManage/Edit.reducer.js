import { _ } from "ag-grid-community";

export const initialState = (IsAdd, datas, siteList) => {
	if (IsAdd) {
		let _specific = SetSpecific(siteList[0].currencyList, null);
		return {
			bonusCode: "",
			promotionType: 1,
			periodFrom: undefined,
			periodTo: undefined,
			expiryDays: "",
			description: "",
			status: 2,
			siteId: siteList[0].siteId,
			chooseCurrencies: [],
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
			chooseCurrencies: Object.keys(datas.specific).map(c => {
				return c;
			}),
			specific: _specific
		};
	}
};

const initSpecific = {
	usage: "0",
	rollover: "",
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
		data.forEach(s => {
			let _newData = {};
			Object.keys(initSpecific).forEach(k => {
				_newData[k] = data[k];
			});
			_specific[s.currencyId] = _newData;
		});
	}

	return _specific;
};

export const defActionType = {
	EDIT_BY_CURRENCY: "EDIT_BY_CURRENCY",
	EDIT_INPUTBOX: "EDIT_INPUTBOX",
	EDIT_CHECKBOX: "EDIT_CHECKBOX",
	SET_CHOOSE_SITE: "SET_CHOOSE_SITE"
};

export function reducer(state, action) {
	switch (action.type) {
		case defActionType.EDIT_BY_CURRENCY:
			return {
				...state,
				specific: {
					...state.specific,
					[action.currencyId]: {
						...state[action.currencyId],
						[action.dataColumn]: action.value
					}
				}
			};

		case defActionType.EDIT_INPUTBOX:
			return {
				...state,
				[action.dataColumn]: action.value
			};

		case defActionType.EDIT_CHECKBOX:
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
		case defActionType.SET_CHOOSE_SITE:
			return {
				...state,
				siteId: action.siteId,
				specific: SetSpecific(action.currencyList)
			};

		default:
			throw new Error();
	}
}
