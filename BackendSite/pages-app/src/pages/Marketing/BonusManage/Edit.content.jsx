import React from "react";
import InputBox from "./Component/InputBox";
import CheckBox from "./Component/Checkbox";
import SelectBox from "./Component/SelectBox";
import DateTimePickerElem from "./Component/DateTimePicker";
import moment from "moment";
import RadioBox from "./Component/RadioBox";
import RichText from "./Component/RichText.jsx";
import { initWalletType, initPromotionType, initWeekOptions, initPeriodType, weekOptions, initSpecialBonusPromotionType, specialBonusOptions, onlyRegisteredBonusOptions, initPromotionDeviceType, initPromotionAction, AutoExtendOptions } from "./defCols";
import { defActionType } from "./Edit.reducer";

const initBonusAmountType = promoType => {
	return Number(promoType) === 3
		? [{ value: 2, displayText: "Amount" }]
		: [
			{ value: 1, displayText: "Percentage" },
			{ value: 2, displayText: "Amount" }
		];
};

export default ({ dispatch, state, action, IsAdd, processing }) => {
	const bonusCodeSwitch = `${state.siteId}-${state.bonusCode}`;
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
				className={`btn btn-add ${state.currentTab === item ? 'active1' : ''}`}
				onClick={action.EditMainValue.bind(this, item, "currentTab")}>
				{window.i18n[`lbl_Currency${item}`]}
			</button>
		);
	});

	const SpecialBonusCheckBox = (value, dataColumn) => {
		if (state.specialBonusCheck.length > 0) {
			dispatch({
				type: defActionType.SPECIALBONUS_VALUE,
				specialBonusCheck: [],
				isSpecial: false
			});
		}
		else {
			dispatch({
				type: defActionType.SPECIALBONUS_VALUE,
				specialBonusCheck: [value],
				isSpecial: true
			});

		}
	};

	const CheckboxSingal = (Title, Options, Selected, OnChange, dataColumn, mainKey, Disabled = false) => {
		let opt = Options.map(o => {
			return <CheckboxSingalCheckBox key={`${Title}-${o.value}${mainKey === null ? '' : `-${mainKey}`}`} onChange={OnChange} Title={o.displayText} checked={Selected.includes(o.value)} Value={o.value} dataColumn={dataColumn} Disabled={Disabled} />
		});
		return (
			<>
				{Title !== "" ? <div className="row-flex">
					<div className="input-item">
						<label className="label">{Title}</label>
					</div>
				</div> : null}
				<div className="row-flex">
					{opt}
				</div>
			</>
		)
	}

	const CheckboxSingalCheckBox = (props) => {
		const SpecialBonusCheckBoxOnChange = (e) => {
			props.onChange(props.Value, props.dataColumn);
		}
		return (
			<label className="checkbox checkbox-checked" >
				<input type="checkbox" value={props.Value} checked={props.checked} onChange={SpecialBonusCheckBoxOnChange} disabled={props.Disabled} />
				<i className="icon icon-checkbox"></i>
				<span className="text">{props.Title}</span>
			</label>
		)
	}

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
							id={bonusCodeSwitch}
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
						<label htmlFor={bonusCodeSwitch}></label>
					</div>
				</div>
			</div>
			<div id="searchForm">
				{
					(state.promotionType == 8) ?
						null : (
							<div className="filter">
								<div className="row-flex">
									{CheckboxSingal("", specialBonusOptions, state.specialBonusCheck, SpecialBonusCheckBox, "specialBonus", null, !IsAdd)}
									{state.specialBonusCheck.length > 0 ?
										<InputBox
											Title="SpecialCode"
											OnChange={action.EditMainValue}
											dataColumn="specialCode"
											value={state.specialCode}
											Disabled={!IsAdd}
										/>
										: null}
								</div>
								{state.specialBonusCheck.length > 0 ?
									<CheckBox
										Options={onlyRegisteredBonusOptions}
										Title=""
										OnChange={action.EditOnlyRegisteredValue}
										dataColumn="onlyRegistered"
										Selected={state.onlyRegistered}
										mainKey={state.siteId}
										Disabled={!IsAdd}
									/>
									: null}
							</div>
						)
				}
				<div className="filter">
					<div className="row-flex">
						<InputBox
							Title="BonusCode"
							OnChange={action.EditMainValue}
							dataColumn="bonusCode"
							value={state.bonusCode}
							Disabled={!IsAdd}
						/>
						<SelectBox
							Title="PromotionType"
							Options={state.isSpecial ? initSpecialBonusPromotionType : initPromotionType}
							HandleChange={action.EditMainValue}
							dataColumn="promotionType"
							SelectedValue={state.promotionType}
							Disabled={!IsAdd}
						/>
						<SelectBox
							Title="PromotionDeviceType"
							Options={initPromotionDeviceType}
							HandleChange={action.EditMainValue}
							dataColumn="promotionDeviceType"
							SelectedValue={state.promotionDeviceType}
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
									} catch (ex) { }
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
									} catch (ex) { }
								}
							}}
						/>
						<InputBox
							Title="Sort"
							OnChange={action.EditMainValue}
							dataColumn="sort"
							value={state.sort}
						/>
						<CheckBox
							Options={AutoExtendOptions}
							Title=""
							OnChange={action.EditAutoExtendValue}
							dataColumn="AutoExtend"
							Selected={state.autoExtend}
							mainKey={state.siteId}
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
					{(state.promotionType == 8) ?
						<RadioBox
							Title="Promotion Action"
							Options={initPromotionAction()}
							OnChange={action.EditMainValue}
							dataColumn="promotionAction"
							Selected={state.promotionAction}
						/>
						: null}
				</div>

				{/* <div className="filter">
						<RadioBox
							Title="Period Type"
							Options={initPeriodType}
							OnChange={action.EditMainValue}
							dataColumn="periodType"
							Selected={state.periodType}
						/>
						{(state.promotionType == 6 || state.promotionType == 7) ?
						(<>
							<div className="row-flex">
								<label className="label">
									Pay Time {" (" + moment().format('Z') + ") "}
									<span className="bule-bold">{state.periodType === 2 ? weekOptions[state.periodWeek] : state.periodType === 3 ? `${state.periodMonth}th of every month` : ''}  {state.redeemHours === null || state.redeemHours === undefined || state.redeemHours === "" ? '' : `${state.redeemHours}:00`}</span>
								</label>
							</div>
							<div className="row-flex">
								{state.periodType === 2 ? (
									<SelectBox
										Title="Week Option"
										Options={initWeekOptions}
										HandleChange={action.EditMainValue}
										dataColumn="periodWeek"
										SelectedValue={state.periodWeek}
									/>
								) : state.periodType === 3 ?
									<InputBox
										Title="Date"
										OnChange={action.EditMainValue}
										dataColumn="periodMonth"
										value={state.periodMonth}
									/>
									: null}
								<InputBox
									Title="Redeem Hours"
									OnChange={action.EditMainValue}
									dataColumn="redeemHours"
									value={state.redeemHours}
								/>
							</div>
						</>
						):null}
				</div>
				{(state.promotionType == 8 || state.promotionType == 6 || state.promotionType == 7) ?
						null : (<div className="filter">
							<div className="row-flex">
								<InputBox
									Title="ExpiryDays"
									OnChange={action.EditMainValue}
									dataColumn="expiryDays"
									value={state.expiryDays}
								/>
							</div>
							<RadioBox
								Title="Wallet Type"
								Options={initWalletType()}
								OnChange={action.GetPromoCategories.bind(
									this,
									false,
									state.walletType
								)}
								dataColumn="walletType"
								Selected={state.walletType}
							/>
							{state.walletType !== null && state.productCategory !== undefined ? (
								<RadioBox
									Title="Product Category"
									Options={state.productCategory}
									OnChange={action.SelectTpId}
									dataColumn={"categoryId"
										// state.walletType === 0 ? "tpId" : "categoryId"
									}
									Selected={state.categoryId
										// state.walletType === 0
										// 	? state.tpId
										// 	: state.categoryId
									}
								/>
							) : null}
						</div>
						)} */}



				{(state.promotionType == 6 || state.promotionType == 7) ?
					(<div className="filter">
						<RadioBox
							Title="Period Type"
							Options={initPeriodType}
							OnChange={action.EditMainValue}
							dataColumn="periodType"
							Selected={state.periodType}
						/>
						<div className="row-flex">
							<label className="label">
								Pay Time {" (" + moment().format('Z') + ") "}
								<span className="bule-bold">{state.periodType === 2 ? weekOptions[state.periodWeek] : state.periodType === 3 ? `${state.periodMonth}th of every month` : ''}  {state.redeemHours === null || state.redeemHours === undefined || state.redeemHours === "" ? '' : `${state.redeemHours}:00`}</span>
							</label>
						</div>
						<div className="row-flex">
							{state.periodType === 2 ? (
								<SelectBox
									Title="Week Option"
									Options={initWeekOptions}
									HandleChange={action.EditMainValue}
									dataColumn="periodWeek"
									SelectedValue={state.periodWeek}
								/>
							) : state.periodType === 3 ?
								<InputBox
									Title="Date"
									OnChange={action.EditMainValue}
									dataColumn="periodMonth"
									value={state.periodMonth}
								/>
								: null}
							<InputBox
								Title="Redeem Hours"
								OnChange={action.EditMainValue}
								dataColumn="redeemHours"
								value={state.redeemHours}
							/>
						</div>
					</div>) :
					(state.promotionType == 8) ?
						null : (<div className="filter">
							<div className="row-flex">
								<InputBox
									Title="ExpiryDays"
									OnChange={action.EditMainValue}
									dataColumn="expiryDays"
									value={state.expiryDays}
								/>
							</div>
							<RadioBox
								Title="Wallet Type"
								Options={initWalletType()}
								OnChange={action.GetPromoCategories.bind(
									this,
									false,
									state.walletType
								)}
								dataColumn="walletType"
								Selected={state.walletType}
							/>
							{state.walletType !== null && state.productCategory !== undefined ? (
								<RadioBox
									Title="Product Category"
									Options={state.productCategory}
									OnChange={action.SelectTpId}
									dataColumn={"categoryId"
										// state.walletType === 0 ? "tpId" : "categoryId"
									}
									Selected={state.categoryId
										// state.walletType === 0
										// 	? state.tpId
										// 	: state.categoryId
									}
								/>
							) : null}
						</div>
				)}
				{(state.promotionType == 2) ? 
					<div className="filter">
								<RadioBox
								Title="Period Type"
								Options={initPeriodType}
								OnChange={action.EditMainValue}
								dataColumn="periodType"
								Selected={state.periodType}
								/>
					</div>:null}

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
								EditCurrencyLangPanelValue={
									action.EditCurrencyLangPanelValue
								}
								EditCurrencyAmountValue={action.EditCurrencyAmountValue.bind(
									this,
									state.currentTab
								)}
								EditTpGameCategoryValue={action.EditTpGameCategoryValue.bind(
									this,
									state.currentTab
								)}
								currencyId={state.currentTab}
								action={action}
							/>
						</div>
					</div>
				) : null}
			</div>
			<div className="filter float-btn-group">
				<button
					className="btn btn-back"
					onClick={action.ClickBack}
					disabled={processing}>
					Back
				</button>
				<button
					className="btn btn-add"
					onClick={action.ClickSubmit}
					disabled={processing}>
					Submit
				</button>
			</div>
		</div>
	);
};

const SubPanel = props => {
	const currencySwitch = `${props.state.siteId}-${props.state.bonusCode}-${props.currencyId}`;
	const langList = Object.keys(props.state.specific[props.currencyId].lang);
	const langOptions = langList.map((lang, i) => {
		return {
			value: lang,
			displayText: lang
		};
	});
	let langId = props.state.specific[props.currencyId].currentLang;
	let tabArr = props.state.specific[props.currencyId].selectedLang.map(
		item => {
			return (
				<button
					key={`${props.currencyId}-tab-${item}`}
					className={`btn btn-add ${langId === item ? 'active1' : ''}`}
					onClick={props.EditMainValue.bind(
						this,
						item,
						"currentLang"
					)}>
					{item}
				</button>
			);
		}
	);

	let tpGameArr = props.state.tpGameCategory.map(
		item => {
			let switchKey = `tpgame-switch-${item.value}`;
			let Data = props.state.specific[props.currencyId].tpGames;
			let Locked = Boolean(Data[item.value]?.status);
			let value = Data[item.value]?.percentage;
			return (
				<div key={`tpgame-${item.value}`} className="list__item" style={Locked ? { alignItems: "center", padding: "0.5rem", background: "#dcffee" } : { alignItems: "center", padding: "0.5rem" }}>
					<label className="">
						<span className="text">{item.displayText}</span>
					</label>
					<div className="switch__container">
						<input id={switchKey} className="switch switch--flat" type="checkbox" checked={Locked} onChange={props.EditTpGameCategoryValue.bind(this, item.displayText, item.value, !Locked, "status")} />
						<label htmlFor={switchKey}></label>
					</div>
					<InputBox
						ClassName="input-item"
						HideTitle={true}
						value={value}
						Disabled={!Locked}
						OnChange={props.EditTpGameCategoryValue.bind(this, item.displayText, item.value)}
						dataColumn="percentage"
						type={'Number'}
						iptSubClass='ratio'
					/>
				</div>
			);
		}
	);

	return (
		<>
			<a
				className="panel-title no-arrow"
				style={{ justifyContent: "space-between" }}>
				<span style={{ color: "white" }}>{props.displayText}</span>
				<div className="switch__container">
					<input
						id={currencySwitch}
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
					<label htmlFor={currencySwitch}></label>
				</div>
			</a>
			<div className="panel-body">
				{(props.state.promotionType == 6 || props.state.promotionType == 7) ?
					(
						<>
							<div className="row-flex">
								<InputBox
									Title="Roll Over"
									OnChange={props.EditMainValue}
									dataColumn="rollOver"
									value={props.state.specific[props.currencyId].rollOver}
								/>
								<InputBox
									Type="Number"
									Title="Max Bonus"
									OnChange={props.EditMainValue}
									dataColumn="maxBonus"
									value={
										props.state.specific[props.currencyId].maxBonus
									}
								/>
								<InputBox
									Type="Number"
									Title="Min Payout"
									OnChange={props.EditMainValue}
									dataColumn="minPayout"
									value={props.state.specific[props.currencyId].minPayout}
								/>
							</div>
							<div className="row-flex ">
								<div class="input-item">
									<label class="label">Tp Games</label>
								</div>
							</div>
							<div className="row-flex ">
								{tpGameArr}
							</div>
						</>
					) :
					(props.state.promotionType == 8) ?
						<InputBox
							Title="Rollover"
							OnChange={props.EditMainValue}
							dataColumn="rollOver"
							value={props.state.specific[props.currencyId].rollOver}
						/>
						: (
							<>
								<InputBox
									Title="Affiliate Id"
									ClassName="form-group"
									inputClass="form-control"
									OnChange={props.EditMainValue}
									dataColumn="affid"
									value={props.state.specific[props.currencyId].affid}
								/>
								<div className="row-flex">
									<InputBox
										Type="Number"
										Title="Usage"
										OnChange={props.EditMainValue}
										dataColumn="usage"
										value={props.state.specific[props.currencyId].usage}
									/>
									<InputBox
										Title="Rollover"
										OnChange={props.EditMainValue}
										dataColumn="rollOver"
										value={props.state.specific[props.currencyId].rollOver}
									/>
									<InputBox
										Title="Total Usage Limit"
										OnChange={props.EditMainValue}
										dataColumn="totalUsageLimit"
										value={props.state.specific[props.currencyId].totalUsageLimit}
									/>
								</div>
								<RadioBox
									Title="Bonus Amount Type"
									Options={initBonusAmountType(props.state.promotionType)}
									OnChange={props.EditCurrencyAmountValue}
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
									{props.state.specific[props.currencyId].bonusAmountType ===
										2 ? null : (
										<InputBox
											Type="Number"
											Title="Max Bonus"
											OnChange={props.EditMainValue}
											dataColumn="maxBonus"
											value={
												props.state.specific[props.currencyId].maxBonus
											}
										/>
									)}
								</div>
								<div className="row-flex">
									<InputBox
										Type="Number"
										Title="Required Amount"
										OnChange={props.EditMainValue}
										dataColumn="requiredAmount"
										value={
											props.state.specific[props.currencyId]
												.requiredAmount
										}
									/>
								</div>
							</>
						)}
				<div className="filter noBGcolor">
					<CheckBox
						Options={langOptions}
						Title="Available Languages"
						OnChange={props.action.EditCurrencyLangCheckBox}
						dataColumn="selectedLang"
						Selected={
							props.state.specific[props.currencyId].selectedLang
						}
						mainKey={props.state.currencyId}
					/>
				</div>
				<div className="filter noBGcolor tag">{tabArr}</div>
				{props.state.specific[props.currencyId].selectedLang.length >
					0 &&
					langId !== null &&
					langId !== "" ? (
					<div className="filter">
						<div className="panel level-1 mt-20">
							<LangPanel
								state={props.state}
								currencyId={props.currencyId}
								EditMainValue={
									props.action.EditCurrencyLangPanelValue
								}
								action={props.action}
								langId={langId}
							/>
						</div>
					</div>
				) : null}
			</div>
		</>
	);
};

const LangPanel = props => {
	const EditDetail = value => {
		props.EditMainValue(value, "detail");
	};
	return (
		<>
			<a
				className="panel-title no-arrow"
				style={{ justifyContent: "space-between" }}>
				<span style={{ color: "white" }}>{props.langId}</span>
			</a>
			<div className="panel-body">
				{(props.state.promotionType == 8 && props.state.promotionAction == 0) ?
					<InputBox
						Title="Promotion Link"
						ClassName="form-group"
						inputClass="form-control"
						OnChange={props.EditMainValue}
						dataColumn="promotionLink"
						value={
							props.state.specific[props.currencyId].lang[
								props.langId
							].promotionLink
						}
					/>
					: null
				}
				<InputBox
					Title="Banner Url"
					ClassName="form-group"
					inputClass="form-control"
					OnChange={props.EditMainValue}
					dataColumn="bannerUrl"
					value={
						props.state.specific[props.currencyId].lang[
							props.langId
						].bannerUrl
					}
				/>
				<InputBox
					Title="Title"
					ClassName="form-group"
					inputClass="form-control"
					OnChange={props.EditMainValue}
					dataColumn="title"
					value={
						props.state.specific[props.currencyId].lang[
							props.langId
						].title
					}
				/>
				<InputBox
					Title="Description"
					ClassName="form-group"
					inputClass="form-control"
					OnChange={props.EditMainValue}
					dataColumn="description"
					value={
						props.state.specific[props.currencyId].lang[
							props.langId
						].description
					}
				/>
				{(props.state.promotionType == 8 && props.state.promotionAction == 0) ?
					null :
					<RichText
						EditDetail={EditDetail}
						Value={
							props.state.specific[props.currencyId].lang[
								props.langId
							].detail
						}
						CurrencyId={props.currencyId}
						LangId={props.langId}
					/>
				}
			</div>
		</>
	);
};