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
		console.log('submit');
	};

	let _options_site = props.SiteList.map(s => {
		return { value: s.siteId, displayText: s.siteName };
	});

	const OnChange = val => {
		let site = props.SiteList.find(s => s.siteId === val);
		dispatch({
			type: defActionType.MAIN_SET_SITE,
			siteId: val,
			currencyList: site.currencyList
		});
	};

	const EditMainValue = (value, dataColumn) => {
		dispatch({ type: defActionType.MAIN_SET_VALUE, value, dataColumn });
	};
	const EditCheckBox = (value, dataColumn) => {
		dispatch({ type: defActionType.MAIN_SET_CHECKBOX, value, dataColumn });
	};

	const EditByCurrency = (value, dataColumn, currencyId) => {
		dispatch({
			type: defActionType.CURRENCY_SET_VALUE,
			value,
			dataColumn,
			currencyId
		});
	};

	return (
		<>
			<div className="filter noBGcolor">
				<RadioBox
					Title="Available Sites"
					Options={_options_site}
					OnChange={OnChange}
					dataColumn="availableSites"
					Selected={state.siteId}
				/>
			</div>
			<Panel
				dispatch={dispatch}
				state={state}
				action={{ ClickSubmit, EditMainValue, EditCheckBox, EditByCurrency }}
				siteList={props.SiteList}
			/>
		</>
	);
};
