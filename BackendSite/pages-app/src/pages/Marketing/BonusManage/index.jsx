import React, { useState, useReducer } from "react";
import InputBox from "./Component/InputBox";
import SelectBox from "./Component/SelectBox";
import DateTimePickerElem from "./Component/DateTimePicker";
import moment from "moment";
import Checkbox from "./Component/Checkbox";
import RadioBox from './Component/RadioBox';

const initialState = {
	dataByLang: { banner: {}, detail: {}, title:{} },
	bonusCode: "",
	promotionType: 1,
	periodFrom: undefined,
	periodTo: undefined,
	expiryDays: "",
	currencies: [],
	availableSites: [],
	usage: "0",
    rollover: "",
    bonus:'',
    bonusAmountType:1
};

const initPromotionType = [
	{ value: 1, displayText: "1st Deposit" },
	{ value: 2, displayText: "Re-Deposit" }
];

const initCurrency = [
	{ value: 1, displayText: "MYR" },
	{ value: 2, displayText: "THB" },
	{ value: 3, displayText: "IDR" }
];

const initSites = [
	{ value: 1, displayText: "Nova88" },
	{ value: 2, displayText: "Bwing" },
	{ value: 3, displayText: "Goal123" }
];

const initBonusAmountType = [
    {value:1, displayText:'Percentage'},
    {value:2, displayText:'Amount'}
]

const defActionType = {
	EDIT_BY_LANG: "EDIT_BY_LANG",
	EDIT_INPUTBOX: "EDIT_INPUTBOX",
	EDIT_CHECKBOX: "EDIT_CHECKBOX"
};

function reducer(state, action) {
	switch (action.type) {
		case defActionType.EDIT_BY_LANG:
			return {
				...state,
				dataByLang: {
					...state.dataByLang,
					[action.dataColumn]: {
						...state[action.dataColumn],
						[action.lang]: action.value
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
						[action.dataColumn]: state[
							action.dataColumn
						].filter(v => v !== action.value)
				  }
				: {
						...state,
						[action.dataColumn]: [
							...state[action.dataColumn],
							action.value
						]
				  };

		default:
			throw new Error();
	}
}

export default props => {
    const [state, dispatch] = useReducer(reducer, initialState);
    const ClickSubmit = ()=>{
        console.log('submit');
    }

	return <Panel dispatch={dispatch} state={state} action={{ClickSubmit}}/>;
};

const Panel = ({ IsAdd, dispatch, state, action }) => {
	const EditInputBox = (value, dataColumn) => {
		dispatch({ type: defActionType.EDIT_INPUTBOX, value, dataColumn });
    };

    const EditCheckBox = (value, dataColumn)=>{
        dispatch({ type: defActionType.EDIT_CHECKBOX, value, dataColumn });
    }
    
	return (
		<div className="container-fluid">
			<h4 className="page-name">Create Bonus</h4>
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
			<DateTimePickerElem
				columnTitle="From"
				dateTimePickerProps={{
					defaultValue: IsAdd
						? ""
						: moment(state.periodFrom).format("l LTS"),
					onBlur: e => {
						try {
							EditInputBox(moment(e).format("l LTS"), "periodFrom");
						} catch (ex) {}
					}
				}}
			/>
			<DateTimePickerElem
				columnTitle="To"
				dateTimePickerProps={{
					defaultValue: IsAdd
						? ""
						: moment(state.periodTo).format("l LTS"),
					onBlur: e => {
						try {
							EditInputBox(moment(e).format("l LTS"), "periodTo");
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
			<Checkbox
				Title="Currency"
				Options={initCurrency}
                OnChange={EditCheckBox}
                dataColumn="currencies"
				Selected={state.currencies}
			/>
			<Checkbox
				Title="Available Sites"
				Options={initSites}
                OnChange={EditCheckBox}
                dataColumn="availableSites"
				Selected={state.availableSites}
			/>
			<InputBox
				Type="Number"
				Title="Usage"
				OnChange={EditInputBox}
				dataColumn="usage"
				value={state.usage}
			/>
			<InputBox
				Title="Rollover"
				OnChange={EditInputBox}
				dataColumn="rollover"
				value={state.rollover}
			/>
            <hr />
            <RadioBox 
                Title="Bonus Amount Type"
				Options={initBonusAmountType}
                OnChange={EditInputBox}
                dataColumn="bonusAmountType"
				Selected={state.bonusAmountType}
            />
            <InputBox
				Type="Number"
				Title="Bonus"
				OnChange={EditInputBox}
				dataColumn="bonus"
				value={state.bonus}
			/>
            <InputBox
				Type="Number"
				Title="Max Bonus"
				OnChange={EditInputBox}
				dataColumn="maxBonus"
				value={state.maxBonus}
			/>
            <hr />
			<LangPanel
				dataByLang={state.dataByLang}
				EditFiled={(value, dataColumn, lang) =>
					dispatch({
						type: defActionType.EDIT_BY_LANG,
						value,
						dataColumn,
						lang
					})
				}
			/>
            <div className="button-group">
            <button className="btn btn-add" style={{marginTop: '20px', marginBottom: '40px', float: 'right'}} onClick={action.ClickSubmit}>Submit</button>
            </div>
		</div>
	);
};

const allLang = ["en", "cs", "th", "id", "vn"];
const LangPanel = ({ dataByLang, EditFiled }) => {
	return allLang.map(lang => {
		return (
			<OuterPanel
				key={`set-panel-${lang}`}
				HeaderContent={
					<span style={{ color: "white" }}>{`${lang}${
						lang === "en" ? "  ( required )" : ""
					}`}</span>
				}
				Content={
					<SettingContent
						banner={
							dataByLang.banner[lang] === undefined
								? ""
								: dataByLang.banner[lang]
                        }
                        title={
                            dataByLang.title[lang] === undefined
								? ""
								: dataByLang.title[lang]
                        }
						detail={
							dataByLang.detail[lang] === undefined
								? ""
								: dataByLang.detail[lang]
						}
						lang={lang}
						EditFiled={EditFiled}
					/>
				}
			/>
		);
	});
};

const OuterPanel = props => {
	return (
		<div className="panel level-1 mt-20">
			<a
				style={{ justifyContent: "space-between" }}
				className="panel-title">
				{props.HeaderContent}
			</a>
			<div className="panel-body">
				<div className="collapse show">{props.Content}</div>
			</div>
		</div>
	);
};

const SettingContent = props => {
	return (
		<div className="panel-body">
			<div className="form-group">
				<label>BannerUrl</label>
				<input
					type="text"
					className="form-control"
					value={props.banner}
					onChange={e => {
						props.EditFiled(e.target.value, "banner", props.lang);
					}}
				/>
			</div>
            <div className="form-group">
				<label>Title</label>
				<input
					type="text"
					className="form-control"
					value={props.title}
					onChange={e => {
						props.EditFiled(e.target.value, "title", props.lang);
					}}
				/>
			</div>
			<div className="form-group">
				<label>Detail</label>
				<textarea
					className="form-control"
					rows="8"
					value={props.detail}
					onChange={e => {
						props.EditFiled(e.target.value, "detail", props.lang);
					}}></textarea>
			</div>
		</div>
	);
};
