import React, { useState, useReducer } from "react";
import InputBox from "./Component/InputBox";
import SelectBox from "./Component/SelectBox";
import DateTimePickerElem from "./Component/DateTimePicker";
import moment from "moment";
import Checkbox from "./Component/Checkbox";
import RadioBox from './Component/RadioBox';
import Index from './index';


const initialState = (newState)=> {
	return newState ?  {
		dataByLang: { banner: {}, detail: {}, title:{} },
		bonusCode: newState.bonusCode,
		promotionType: newState.promotionType,
		periodFrom:  newState.periodFrom,
		periodTo: newState.periodTo,
		expiryDays: "",
		currencies: [],
		availableSites: 1,
		usage: "0",
		rollover: "",
		bonus:'',
		bonusAmountType:1
	} : {
	dataByLang: { banner: {}, detail: {}, title:{} },
	bonusCode: "",
	promotionType: 1,
	periodFrom: undefined,
	periodTo: undefined,
	expiryDays: "",
	currencies: [],
	availableSites: 1,
	usage: "0",
    rollover: "",
    bonus:'',
    bonusAmountType:1
}};

const initPromotionType = [
	{ value: 1, displayText: "1st Deposit" },
	{ value: 2, displayText: "Re-Deposit" }
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
	EDIT_CHECKBOX: "EDIT_CHECKBOX",
	SET_PROPS_DATA:"SET_PROPS_DATA"
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
						[action.lang]: action.value.dataColumn
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
	const [state, dispatch] = useReducer(reducer, initialState(props.Datas));
    const ClickSubmit = ()=>{
        console.log('submit');
	}

	return <Panel dispatch={dispatch} state={state} action={{ClickSubmit}}/>;
};

const Panel = ({ IsAdd, dispatch, state, action }, props) => {
	const [ backIndex, setBackIndex ] = useState(false)
	const EditInputBox = (value, dataColumn) => {
		dispatch({ type: defActionType.EDIT_INPUTBOX, value, dataColumn });
    };

    const EditCheckBox = (value, dataColumn) => {
        dispatch({ type: defActionType.EDIT_CHECKBOX, value, dataColumn });
	}
	
	return (
		backIndex ? <Index />:
		<div className="container-fluid" id="Panel">
            <div className="filter noBGcolor">
				<RadioBox
					Title="Available Sites"
					Options={initSites}
					OnChange={EditInputBox}
					dataColumn="availableSites"
					Selected={state.availableSites}
				/>
            </div>
			<div className="panel level-1 mt-20">
            <div className="panel-title no-arrow" style={{justifyContent: 'space-between'}}>
                <span style={{color: 'white'}}>Bonus Info</span>
                <div className="switch__container">
                    <input id="switch1" className="switch switch--flat" type="checkbox" />
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
				</div>
            </div>
            <div className="filter">
                <div className="panel level-1 mt-20">

                </div>
            </div>
        </div>
		<div className="filter float-btn-group">
			<button className="btn btn-back" onClick={()=>{setBackIndex(true)}}>Back</button>
			<button className="btn btn-add" onClick={action.ClickSubmit}>Submit</button>
		</div>
	</div>
	);
};

const SubPanel = props => {
	return(
		<>
		                    <a className="panel-title no-arrow" style={{justifyContent: 'space-between'}}>
                        <span style={{color: 'white'}}>MYR</span>
                        <div className="switch__container">
                            <input id="switch2" className="switch switch--flat" type="checkbox" />
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
							Selected={props.state.bonusAmountType}
						/>
                        <div className="row-flex">
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
                        </div>
                        <div className="row-flex"></div>
                        <div className="form-group">
                            <label>Banner Url</label>
                            <input type="text" className="form-control" value="" />
                        </div>
                        <div className="form-group">
                            <label>Title</label>
                            <input type="text" className="form-control" value="" />
                        </div>
                        <div className="form-group">
                            <label>Detail</label>
                            <textarea className="form-control" rows="8"></textarea>
                        </div>
                    </div>
		</>
	)
}