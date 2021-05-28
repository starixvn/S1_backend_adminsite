import moment from "moment";
export const defActionType = {
	CURRENCY_SET_VALUE: "CURRENCY_SET_VALUE",
	
};

export const initialState = (datas) => {
		
		// let _selectedCurrencies = datas.specific.map(s => {
		// 	return s.currencyId;
		// });
		return {
			
			
		};
	}
	
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
	
			default:
				throw new Error();
		}
	}
