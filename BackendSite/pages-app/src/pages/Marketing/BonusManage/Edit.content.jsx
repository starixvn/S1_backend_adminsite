import React from "react";
import InputBox from "./Component/InputBox";
import CheckBox from "./Component/Checkbox";
import SelectBox from "./Component/SelectBox";
import DateTimePickerElem from "./Component/DateTimePicker";
import { defActionType } from "./Edit.reducer";
import moment from "moment";
import RadioBox from "./Component/RadioBox";

const initPromotionType = [
	{ value: 1, displayText: "1st Deposit" },
	{ value: 2, displayText: "Re-Deposit" }
];

const initBonusAmountType = [
	{ value: 1, displayText: "Percentage" },
	{ value: 2, displayText: "Amount" }
];

export default ({ IsAdd, state, action, siteList }) => {
	const currencyList = Object.keys(state.specific);
	const currencyOptions = currencyList.map(i => {
		return {
			value: Number(i),
			displayText: window.i18n[`lbl_Currency${i}`]
		};
	});

	let tabArr = state.selectedCurrencies.map(item => {
		return (
			<button
				key={`${state.siteId}-tab-${item}`}
				className="btn btn-add active"
				onClick={action.EditMainValue.bind(this, item, "currentTab")}>
				{window.i18n[`lbl_Currency${item}`]}
			</button>
		);
	});

	return (
		<div className="container-fluid" id="Panel">
			<div className="filter noBGcolor"></div>
			<div className="panel level-1 mt-20">
				<div
					className="panel-title no-arrow"
					style={{ justifyContent: "space-between" }}>
					<span style={{ color: "white" }}>Bonus Info</span>
					<div className="switch__container">
						<input
							id="switch1"
							className="switch switch--flat"
							type="checkbox"
							onChange={action.EditMainValue.bind(
								this,
								state.status === 1 ? 2 : 1,
								"status"
							)}
							value={state.status}
							checked={state.status === 1}
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
							OnChange={action.EditMainValue}
							dataColumn="bonusCode"
							value={state.bonusCode}
						/>
						<SelectBox
							Title="PromotionType"
							Options={initPromotionType}
							HandleChange={action.EditMainValue}
							dataColumn="promotionType"
							SelectedValue={state.promotionType}
						/>
					</div>
					<div className="row-flex">
						<DateTimePickerElem
							columnTitle="From"
							dateTimePickerProps={{
								defaultValue: moment(state.periodFrom).format(
									"l LTS"
								),
								onBlur: e => {
									try {
										action.EditMainValue(
											moment(e).format("l LTS"),
											"periodFrom"
										);
									} catch (ex) {}
								}
							}}
						/>
						<DateTimePickerElem
							columnTitle="To"
							dateTimePickerProps={{
								defaultValue: moment(state.periodTo).format(
									"l LTS"
								),
								onBlur: e => {
									try {
										action.EditMainValue(
											moment(e).format("l LTS"),
											"periodTo"
										);
									} catch (ex) {}
								}
							}}
						/>
						<InputBox
							Title="ExpiryDays"
							OnChange={action.EditMainValue}
							dataColumn="expiryDays"
							value={state.expiryDays}
						/>
					</div>
					<InputBox
						Title="Description"
						ClassName="form-group"
						inputClass="form-control"
						OnChange={action.EditMainValue}
						dataColumn="description"
						value={state.description}
					/>
				</div>
				<div className="filter noBGcolor">
					<CheckBox
						Options={currencyOptions}
						Title="Available Currency"
						OnChange={action.EditCurrencyCheckBox}
						dataColumn="selectedCurrencies"
						Selected={state.selectedCurrencies}
						mainKey={state.siteId}
					/>
				</div>

				<div className="filter noBGcolor tag">{tabArr}</div>
				{state.selectedCurrencies.length > 0 &&
				state.currentTab !== null ? (
					<div className="filter">
						<div className="panel level-1 mt-20">
							<SubPanel
								state={state}
								displayText={
									window.i18n[
										`lbl_Currency${state.currentTab}`
									]
								}
								EditMainValue={action.EditCurrencyPanel.bind(
									this,
									state.currentTab
								)}
								currencyId={state.currentTab}
							/>
						</div>
					</div>
				) : null}
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
    const EditDetail = (e)=>{
        props.EditMainValue(
            e.target.value,
            "detail"
        )
    }
	return (
		<>
			<a
				className="panel-title no-arrow"
				style={{ justifyContent: "space-between" }}>
				<span style={{ color: "white" }}>{props.displayText}</span>
				<div className="switch__container">
					<input
						id="switch2"
						className="switch switch--flat"
						type="checkbox"
						onChange={props.EditMainValue.bind(
							this,
							props.state.specific[props.currencyId].status === 1
								? 2
								: 1,
							"status"
						)}
						value={props.state.specific[props.currencyId].status}
						checked={
							props.state.specific[props.currencyId].status === 1
						}
					/>
					<label htmlFor="switch2"></label>
				</div>
			</a>
			<div className="panel-body">
				<div className="row-flex">
					<InputBox
						Type="Number"
						Title="Usage"
						OnChange={props.EditMainValue}
						dataColumn="usage"
						value={props.state.specific[props.currencyId].usage}
					/>
					<InputBox
						Title="rollOver"
						OnChange={props.EditMainValue}
						dataColumn="rollOver"
						value={props.state.specific[props.currencyId].rollOver}
					/>
				</div>
				<RadioBox
					Title="Bonus Amount Type"
					Options={initBonusAmountType}
					OnChange={props.EditMainValue}
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
						OnChange={props.EditMainValue}
						dataColumn="bonus"
						value={props.state.specific[props.currencyId].bonus}
						iptSubClass={
							props.state.specific[props.currencyId]
								.bonusAmountType === 1
								? "ratio"
								: null
						}
					/>
					<InputBox
						Type="Number"
						Title="Max Bonus"
						OnChange={props.EditMainValue}
						dataColumn="maxBonus"
						value={props.state.specific[props.currencyId].maxBonus}
					/>
				</div>
				<div className="row-flex"></div>
				<InputBox
					Title="Banner Url"
					ClassName="form-group"
					inputClass="form-control"
					OnChange={props.EditMainValue}
					dataColumn="bannerUrl"
					value={props.state.specific[props.currencyId].bannerUrl}
				/>
				<InputBox
					Title="Title"
					ClassName="form-group"
					inputClass="form-control"
					OnChange={props.EditMainValue}
					dataColumn="title"
					value={props.state.specific[props.currencyId].title}
				/>
				<div className="form-group">
					<label>Detail</label>
					<textarea
						className="form-control"
						rows="8"
						onChange={EditDetail}
						value={
							props.state.specific[props.currencyId].detail
						}></textarea>
				</div>
			</div>
		</>
	);
};
