import React, { useState, useReducer } from 'react';
import InputBox from './Component/InputBox';
import SelectBox from './Component/SelectBox';
import DateTimePickerElem from './Component/DateTimePicker';
import moment from 'moment';
import RadioBox from './Component/RadioBox';
import Index from './index';

const initIsAddState = siteList => {
	let _specific = {};
	let _data = {};
	let _arr = [];

	if (siteList) {
		siteList.forEach(site => {
			// site.currencyList.forEach(currency => {
			// 	_specific[currency] = {
			// 		usage: '0',
			// 		rollover: '',
			// 		bonusAmountType: 1,
			// 		bonus: '',
			// 		maxBonus: '',
			// 		bannerUrl: '',
			// 		title: '',
			// 		detail: '',
			// 		status: 2
			// 	};
			// });
			// _specific=SetSpecific
			_data[site] = {
				bonusCode: '',
				promotionType: 1,
				periodFrom: undefined,
				periodTo: undefined,
				expiryDays: '',
				description: '',
				status: 2,
				siteId: site.siteId,
				specific: _specific
			};

			_arr.push(_data[site]);
		});
		return _arr;
	}
};

const initialState = (IsAdd, newState, siteList) => {
	if (IsAdd) {
		let _specific = SetSpecific(siteList[0].currencyList);
		let _data = {};
		let _arr = [];

		// siteList.forEach(site => {
		// 	site.currencyList.forEach(currency => {
		// 		_specific[currency] = {
		// 			usage: '0',
		// 			rollover: '',
		// 			bonusAmountType: 1,
		// 			bonus: '',
		// 			maxBonus: '',
		// 			bannerUrl: '',
		// 			title: '',
		// 			detail: '',
		// 			status: 2
		// 		};
		// 	});

		// 	_data[site] = {
		// 		bonusCode: '',
		// 		promotionType: 1,
		// 		periodFrom: undefined,
		// 		periodTo: undefined,
		// 		expiryDays: '',
		// 		description: '',
		// 		status: 2,
		// 		siteId: site.siteId,
		// 		specific: _specific
		// 	};

		// 	_arr.push(_data[site]);
		// 	return _arr;
		// });

		return {
			bonusCode: '',
			promotionType: 1,
			periodFrom: undefined,
			periodTo: undefined,
			expiryDays: '',
			description: '',
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

const SetSpecific = (currencyList)=>{
	let _specific = {};
	currencyList.forEach(currency => {
		_specific[currency] = {
			usage: '0',
			rollover: '',
			bonusAmountType: 1,
			bonus: '',
			maxBonus: '',
			bannerUrl: '',
			title: '',
			detail: '',
			status: 2
		};
	})

	return _specific;
}

const initPromotionType = [
	{ value: 1, displayText: '1st Deposit' },
	{ value: 2, displayText: 'Re-Deposit' }
];

// const initSites = [
// 	{ value: 1, displayText: 'Nova88' },
// 	{ value: 2, displayText: 'Bwing' },
// 	{ value: 3, displayText: 'Goal123' }
// ];

const initBonusAmountType = [
	{ value: 1, displayText: 'Percentage' },
	{ value: 2, displayText: 'Amount' }
];

const defActionType = {
	EDIT_BY_CURRENCY: 'EDIT_BY_CURRENCY',
	EDIT_INPUTBOX: 'EDIT_INPUTBOX',
	EDIT_CHECKBOX: 'EDIT_CHECKBOX'
};

function reducer(state, action) {
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
				siteId: //siteId,
				specific: SetSpecific(currencyList)
			}

		default:
			throw new Error();
	}
}

export default props => {
	const [state, dispatch] = useReducer(
		reducer,
		initialState(props.IsAdd, props.Datas, props.SiteList)
	);
	let _state = {};
	//const [chooseSite, setChooseSite] = useState(1);

	let _initIsAddState = initIsAddState(props.SiteList);
	const [isAddState] = useState(_initIsAddState);
	_state = isAddState[chooseSite - 1];
	console.log(_state);

	const ClickSubmit = () => {
		console.log('submit');
	};

	let _options_site = props.SiteList.map(s => {
		return { value: s.siteId, displayText: s.siteName };
	});

	const OnChange = val => {
		let site = props.SiteList.find(s=> s.siteId === val);
		dispatch({type:'', siteId:val, currencyList: site.currencyList});
		//setChooseSite(val);
	};

	return (
		<>
			<div className="filter noBGcolor">
				<RadioBox
					Title="Available Sites"
					Options={_options_site}
					OnChange={OnChange}
					dataColumn="availableSites"
					Selected={chooseSite}
				/>
			</div>
			<Panel
				dispatch={dispatch}
				state={props.IsAdd ? _state : state}
				action={{ ClickSubmit, ...props.Action }}
				siteList={props.SiteList}
			/>
		</>
	);
};

const Panel = ({ IsAdd, dispatch, state, action, siteList }) => {
	const EditInputBox = (value, dataColumn) => {
		dispatch({ type: defActionType.EDIT_INPUTBOX, value, dataColumn });
	};

	const EditByCurrency = (value, dataColumn, currencyId) => {
		dispatch({
			type: defActionType.EDIT_BY_LANG,
			value,
			dataColumn,
			currencyId
		});
	};

	let _options_site = siteList.map(s => {
		return { value: s.siteId, displayText: s.siteName };
	});

	const currencyList = Object.keys(state.specific);
	let arr = currencyList.map(i => {
		return (
			<div className="panel level-1 mt-20" key={`currencyList-${i}`}>
				<SubPanel
					state={state}
					EditInputBox={EditByCurrency}
					displayText={window.i18n[`lbl_Currency${i}`]}
					currencyId={i}
					_options_site={_options_site}
				/>
			</div>
		);
	});

	return (
		<div className="container-fluid" id="Panel">
			<div className="filter noBGcolor">
				{/* <RadioBox
					Title="Available Sites"
					Options={_options_site}
					OnChange={EditInputBox}
					dataColumn="availableSites"
					Selected={state.availableSites}
					mainKey={state.siteId}
				/> */}
			</div>
			<div className="panel level-1 mt-20">
				<div
					className="panel-title no-arrow"
					style={{ justifyContent: 'space-between' }}>
					<span style={{ color: 'white' }}>Bonus Info</span>
					<div className="switch__container">
						<input
							id="switch1"
							className="switch switch--flat"
							type="checkbox"
						/>
						<label htmlFor="switch1"></label>
					</div>
				</div>
			</div>

			<div id="searchForm">
				<div className="filter">
					<div className="row-flex">
						<InputBox
							Title="BonusCode"
							OnChange={EditInputBox}
							dataColumn="bonusCode"
							value={state.bonusCode}
						/>
						<SelectBox
							Title="PromotionType"
							Options={initPromotionType}
							HandleChange={EditInputBox}
							dataColumn="promotionType"
							SelectedValue={state.promotionType}
						/>
					</div>
					<div className="row-flex">
						<DateTimePickerElem
							columnTitle="From"
							dateTimePickerProps={{
								defaultValue: IsAdd
									? ''
									: moment(state.periodFrom).format('l LTS'),
								onBlur: e => {
									try {
										EditInputBox(
											moment(e).format('l LTS'),
											'periodFrom'
										);
									} catch (ex) {}
								}
							}}
						/>
						<DateTimePickerElem
							columnTitle="To"
							dateTimePickerProps={{
								defaultValue: IsAdd
									? ''
									: moment(state.periodTo).format('l LTS'),
								onBlur: e => {
									try {
										EditInputBox(
											moment(e).format('l LTS'),
											'periodTo'
										);
									} catch (ex) {}
								}
							}}
						/>
						<InputBox
							Title="ExpiryDays"
							OnChange={EditInputBox}
							dataColumn="expiryDays"
							value={state.expiryDays}
						/>
					</div>
				</div>
				<div className="filter">{arr}</div>
			</div>
			<div className="filter float-btn-group">
				<button className="btn btn-back" onClick={action.ClickBack}>
					Back
				</button>
				<button className="btn btn-add" onClick={action.ClickSubmit}>
					Submit
				</button>
			</div>
		</div>
	);
};

const SubPanel = props => {
	return (
		<>
			<a
				className="panel-title no-arrow"
				style={{ justifyContent: 'space-between' }}>
				<span style={{ color: 'white' }}>{props.displayText}</span>
				<div className="switch__container">
					<input
						id="switch2"
						className="switch switch--flat"
						type="checkbox"
					/>
					<label htmlFor="switch2"></label>
				</div>
			</a>
			<div className="panel-body">
				<div className="row-flex">
					<InputBox
						Type="Number"
						Title="Usage"
						OnChange={props.EditInputBox}
						dataColumn="usage"
						value={props.state.usage}
					/>
					<InputBox
						Title="Rollover"
						OnChange={props.EditInputBox}
						dataColumn="rollover"
						value={props.state.rollover}
					/>
				</div>
				<RadioBox
					Title="Bonus Amount Type"
					Options={initBonusAmountType}
					OnChange={props.EditInputBox}
					dataColumn="bonusAmountType"
					Selected={
						props.state.specific[props.currencyId].bonusAmountType
					}
					mainKey={props.currencyId}
				/>
				<div className="row-flex">
					<InputBox
						Type="Number"
						Title="Bonus"
						OnChange={props.EditInputBox}
						dataColumn="bonus"
						value={props.state.bonus}
					/>
					<InputBox
						Type="Number"
						Title="Max Bonus"
						OnChange={props.EditInputBox}
						dataColumn="maxBonus"
						value={props.state.maxBonus}
					/>
				</div>
				<div className="row-flex"></div>
				<InputBox
					Title="Banner Url"
					ClassName="form-group"
					inputClass="form-control"
					OnChange={props.EditInputBox}
					dataColumn="bannerUrl"
					value={props.state.specific.bannerUrl}
				/>
				<InputBox
					Title="Title"
					ClassName="form-group"
					inputClass="form-control"
					OnChange={props.EditInputBox}
					dataColumn="title"
					value={props.state.specific.title}
				/>
				<div className="form-group">
					<label>Detail</label>
					<textarea
						className="form-control"
						rows="8"
						onChange={props.EditInputBox}
						value={props.state.specific.detail}></textarea>
				</div>
			</div>
		</>
	);
};
