import React, { useState, useReducer } from 'react';
import InputBox from './Component/InputBox';
import SelectBox from './Component/SelectBox';
import DateTimePickerElem from './Component/DateTimePicker';
import moment from 'moment';
import RadioBox from './Component/RadioBox';
import {reducer, defActionType, initialState} from './Edit.reducer'

const initPromotionType = [
	{ value: 1, displayText: '1st Deposit' },
	{ value: 2, displayText: 'Re-Deposit' }
];

const initBonusAmountType = [
	{ value: 1, displayText: 'Percentage' },
	{ value: 2, displayText: 'Amount' }
];

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
		let site = props.SiteList.find(s=> s.siteId === val);
		dispatch({type:'', siteId:val, currencyList: site.currencyList});
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
