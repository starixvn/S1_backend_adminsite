import React, { useReducer, useEffect, useRef } from "react";
import { reducer, defActionType, initialState } from "./Edit.reducer";
import Panel from "./Edit.content";

export default props => {
	const isMountedRef = useRef(null);
	useEffect(() => {
		isMountedRef.current = true;
		return () => (isMountedRef.current = false);
	});
	const [state, dispatch] = useReducer(
		reducer,
		initialState(props.IsAdd, props.Datas, props.SetSite, props.SetCurrencyList, props.SetLangList)
	);
	const ClickSubmit = () => {
		let checked = true;
		let skipField = props.skipField.default;
		for (let key in state) {
			if (key === "specific") {
				for (let c_key in state[key]) {
					if (state.selectedCurrencies.includes(Number(c_key))) {
						for (let f_key in state[key][c_key]) {
							if (skipField.includes(f_key)) continue;
							let column = state[key][c_key][f_key];
							if (
								column === null ||
								column === undefined ||
								column === ""
							) {
								alert(
									`${window.i18n[`lbl_Currency${c_key}`]
									}'s ${f_key} does not allow null.`
								);
								checked = false;
								break;
							}
						}
					}

					if (!checked) {
						break;
					}
				}
			} else if (
				state[key] === null ||
				state[key] === undefined ||
				state[key] === ""
			) {
				if (skipField.includes(key)) continue;
				alert(`${key} does not allow null.`);
				checked = false;
				break;
			} else if (
				key === "selectedCurrencies" &&
				state[key].length === 0
			) {
				alert("Please select at least one currency.");
				checked = false;
				break;
			}

			if (!checked) {
				break;
			}
		}
		if (checked) {
			props.action.SetBanner(state);
		}
	};

	const EditMainValue = (value, dataColumn) => {
		dispatch({ type: defActionType.MAIN_SET_VALUE, value, dataColumn });
	};

	const EditCurrencyCheckBox = (value, dataColumn) => {
		dispatch({
			type: defActionType.MAIN_SET_CURRENCY_CHECKBOX,
			value,
			dataColumn
		});
	};

	const EditCurrencyPanel = (currencyId, value, dataColumn) => {
		dispatch({
			type: defActionType.CURRENCY_SET_VALUE,
			value,
			dataColumn,
			currencyId
		});
	};

	const EditCurrencyLangPanelValue = (value, dataColumn) => {
		dispatch({
			type: defActionType.CURRENCY_LANG_SET_VALUE,
			value,
			dataColumn
		});
	};

	const EditCurrencyLangCheckBox = (value, dataColumn) => {
		dispatch({
			type: defActionType.CURRENCY_SET_LANG_CHECKBOX,
			value,
			dataColumn
		});
	};

	return (
		<Panel
			dispatch={dispatch}
			state={state}
			action={{
				ClickSubmit,
				EditMainValue,
				EditCurrencyCheckBox,
				EditCurrencyPanel,
				ClickBack: props.action.ClickBack,
				EditCurrencyLangPanelValue,
				EditCurrencyLangCheckBox,
			}}
			IsAdd={props.IsAdd}
			processing={props.processing}
		/>
	);
};

