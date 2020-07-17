export const initialState = (IsAdd, newState, siteList) => {
	if (IsAdd) {
		let _specific = SetSpecific(siteList[0].currencyList);

		return {
			bonusCode: "",
			promotionType: 1,
			periodFrom: undefined,
			periodTo: undefined,
			expiryDays: "",
			description: "",
			status: 2,
			siteId: siteList[0].siteId,
			specific: _specific
		};
	} else {
		return {
			bonusCode: newState.bonusCode,
			promotionType: newState.promotionType,
			periodFrom: newState.periodFrom,
			periodTo: newState.periodTo,
			expiryDays: newState.expiryDays,
			description: newState.description,
			status: newState.status,
			siteId: newState.siteId,
			specific: {}
		};
	}
};

const SetSpecific = currencyList => {
	let _specific = {};
	currencyList.forEach(currency => {
		_specific[currency] = {
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
	});

	return _specific;
};

export const defActionType = {
	EDIT_BY_CURRENCY: "EDIT_BY_CURRENCY",
	EDIT_INPUTBOX: "EDIT_INPUTBOX",
	EDIT_CHECKBOX: "EDIT_CHECKBOX"
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
		case "setChooseSite":
			//siteId
			//specific => SetSpecific(currencyList)
			return {
				...state,
				siteId: 1,
				specific: SetSpecific(action.currencyList)
			};

		default:
			throw new Error();
	}
}
