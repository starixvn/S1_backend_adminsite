import React, { useReducer } from 'react';
import RadioBox from './Component/RadioBox';
import { reducer, defActionType, initialState } from './Edit.reducer';
import Panel from './Edit.content'

export default props => {
	const [state, dispatch] = useReducer(
		reducer,
		initialState(props.IsAdd, props.Datas, props.SiteList)
	);

	const ClickSubmit = () => {
		let checked = true;
		for(let key in state){
			if(key === 'specific'){
				for(let c_key in state[key]){
					if(state.selectedCurrencies.includes(c_key)){
						for(let f_key in state[key][c_key]){
							let column = state[key][c_key][f_key];
							if(column===null || column===undefined || column===''){
								alert(`${window.i18n[`lbl_Currency${c_key}`]}'s ${f_key} does not allow null.`);
								checked=false;
								break;
							}
						}
					}

					if(!checked){
						break;
					}
				}
			}else if(state[key]===null || state[key]===undefined || state[key]===''){
				alert(`${key} does not allow null.`);
				checked = false;
				break;
			}else if(key==='selectedCurrencies' && state[key].length === 0){
				alert("Please select at least one currency.");
				checked = false;
				break;
			}

			if(!checked){
				break;
			}
		}

		if(checked){
			props.action.SetPromotion(state);
		}
	};

	let _options_site = props.SiteList.map(s => {
		return { value: s.siteId, displayText: s.siteName };
	});

	const SelectSite = val => {
		let site = props.SiteList.find(s => s.siteId === val);
		dispatch({
			type: defActionType.MAIN_SET_SITE,
			siteId: val,
			currencyList: site.currencyList,
			langList: site.langList
		});
	};

	const EditMainValue = (value, dataColumn) => {
		dispatch({ type: defActionType.MAIN_SET_VALUE, value, dataColumn });
	};

	const EditCurrencyCheckBox = (value, dataColumn) => {
		dispatch({ type: defActionType.MAIN_SET_CURRENCY_CHECKBOX, value, dataColumn });
	};

	const EditCurrencyPanel = (currencyId, value, dataColumn) => {
		dispatch({
			type: defActionType.CURRENCY_SET_VALUE,
			value,
			dataColumn,
			currencyId
		});
	};

	const EditCurrencyLangPanelValue = (value, dataColumn) =>{
		dispatch({
			type: defActionType.CURRENCY_LANG_SET_VALUE,
			value,
			dataColumn
		});
	}

	const EditCurrencyLangCheckBox = (value, dataColumn) =>{
		dispatch({type:defActionType.CURRENCY_SET_LANG_CHECKBOX, value, dataColumn});
	}

	return (
		<>
			<div className="filter noBGcolor">
				<RadioBox
					Title="Available Sites"
					Options={_options_site}
					OnChange={SelectSite}
					dataColumn="availableSites"
					Selected={state.siteId}
				/>
			</div>
			<Panel
				dispatch={dispatch}
				state={state}
				action={{ ClickSubmit, EditMainValue, EditCurrencyCheckBox, EditCurrencyPanel, ClickBack:props.action.ClickBack, EditCurrencyLangPanelValue, EditCurrencyLangCheckBox}}
				siteList={props.SiteList}
				IsAdd={props.IsAdd}
			/>
		</>
	);
};
