import React from "react";
import InputBox from "./Component/InputBox";
import CheckBox from "./Component/Checkbox";
import SelectBox from "./Component/SelectBox";
import RadioBox from "./Component/RadioBox";
import DateTimePickerElem from "./Component/DateTimePicker";
import moment from "moment";
import { initBannerType, initActionModeType, initShowStatusType } from "./defCols";

export default ({ state, action, IsAdd, processing }) => {
	const newActionModeType = initActionModeType
		.filter(i => {
			if (state.bannerType == 2 && i.value !== 2) {
				return false;
			}
			if (state.bannerType == 3 && i.value === 3) {
				return false;
			}
			return true
		}).map(i => {
			return i;
		});


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
	// console.log(state.bannerType);
	return (
		<div className="container-fluid" id="Panel">
			<div className="filter noBGcolor"></div>
			<div className="panel level-1 mt-20">
				<div
					className="panel-title no-arrow"
					style={{ justifyContent: "space-between" }}>
					<span style={{ color: "white" }}>Banner Info</span>
				</div>
			</div>
			<div id="searchForm">
				<div className="filter">
					<div className="row-flex">
						<InputBox
							Title="Banner Name"
							OnChange={action.EditMainValue}
							dataColumn="bannerName"
							value={state.bannerName}
							Disabled={!IsAdd}
						/>
						<SelectBox
							Title="Banner Type"
							Options={initBannerType}
							HandleChange={action.EditMainValue}
							dataColumn="bannerType"
							SelectedValue={state.bannerType}
							Disabled={!IsAdd}
						/>
						<InputBox
							Title="Sort"
							OnChange={action.EditMainValue}
							dataColumn="sort"
							value={state.sort}
							type="number"
						/>
					</div>
				</div>
				<div className="filter">
					<RadioBox
						Title="Action Mode"
						Options={newActionModeType}
						OnChange={action.EditMainValue}
						dataColumn="actionMode"
						Selected={state.actionMode}
					/>
					<RadioBox
						Title="Show Status"
						Options={initShowStatusType}
						OnChange={action.EditMainValue}
						dataColumn="showStatus"
						Selected={state.showStatus}
					/>
					<div className="row-flex">
						<DateTimePickerElem
							columnTitle="Start Time"
							dateTimePickerProps={{
								defaultValue: moment(state.startTime).format(
									"l LTS"
								),
								onBlur: e => {
									try {
										action.EditMainValue(
											moment(e).format("l LTS"),
											"startTime"
										);
									} catch (ex) { }
								}
							}}
						/>
						<DateTimePickerElem
							columnTitle="End Time"
							dateTimePickerProps={{
								defaultValue: moment(state.endTime).format(
									"l LTS"
								),
								onBlur: e => {
									try {
										action.EditMainValue(
											moment(e).format("l LTS"),
											"endTime"
										);
									} catch (ex) { }
								}
							}}
						/>
					</div>
					<div className="row-flex">
						{
							state.bannerType == 1 ?
								state.actionMode === 2 ?
									<>
										<InputBox
											Title="Tp Id"
											OnChange={action.EditMainValue}
											dataColumn="tpId"
											value={state.tpId}
										/>
										<InputBox
											Title="Game Id"
											OnChange={action.EditMainValue}
											dataColumn="gameId"
											value={state.gameId}
										/>
									</>
									: state.actionMode === 3 ?
										<InputBox
											Title="Bonus Code"
											OnChange={action.EditMainValue}
											dataColumn="bonusCode"
											value={state.bonusCode}
										/>
										:state.actionMode === 6 ?
										<InputBox
											Title="Url Router"
											OnChange={action.EditMainValue}
											dataColumn="urlRouter"
											value={state.urlRouter}
										/>
										: null
								: state.bannerType == 2 ?
									state.actionMode === 2 ?
										<>
											<InputBox
												Title="Match Id"
												OnChange={action.EditMainValue}
												dataColumn="matchId"
												value={state.matchId}
											/>
											<InputBox
												Title="Tp Id"
												OnChange={action.EditMainValue}
												dataColumn="tpId"
												value={state.tpId}
											/>
											<InputBox
												Title="Game Id"
												OnChange={action.EditMainValue}
												dataColumn="gameId"
												value={state.gameId}
											/>
										</>
										: null
									: state.bannerType == 3 ?
										state.actionMode === 2 ?
											<>
												<InputBox
													Title="Tp Id"
													OnChange={action.EditMainValue}
													dataColumn="tpId"
													value={state.tpId}
												/>
												<InputBox
													Title="Game Id"
													OnChange={action.EditMainValue}
													dataColumn="gameId"
													value={state.gameId}
												/>
											</>
											: null
										: null
						}
					</div>
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
									displayText={window.i18n[`lbl_Currency${state.currentTab}`]}
									EditMainValue={action.EditCurrencyPanel.bind(this, state.currentTab)}
									EditCurrencyLangPanelValue={action.EditCurrencyLangPanelValue}
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
	const currencySwitch = `${props.state.siteId}-${props.state.bannerName}-${props.currencyId}`;
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

	return (
		<>
			<a className="panel-title no-arrow" style={{ justifyContent: "space-between" }}>
				<span style={{ color: "white" }}>{props.displayText}</span>
				<div className="switch__container">
					<input
						id={currencySwitch}
						className="switch switch--flat"
						type="checkbox"
						onChange={props.EditMainValue.bind(this, props.state.specific[props.currencyId].status === 0 ? 1 : 0, "status")}
						value={props.state.specific[props.currencyId].status}
						checked={props.state.specific[props.currencyId].status === 1}
					/>
					<label htmlFor={currencySwitch}></label>
				</div>
			</a>
			<div className="panel-body">
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
	return (
		<>
			<a
				className="panel-title no-arrow"
				style={{ justifyContent: "space-between" }}>
				<span style={{ color: "white" }}>{props.langId}</span>
			</a>
			<div className="panel-body">
				{
					props.state.actionMode === 4 || props.state.actionMode === 5 ?
						<InputBox
							Title="Banner Link"
							ClassName="form-group"
							inputClass="form-control"
							OnChange={props.EditMainValue}
							dataColumn="bannerLink"
							value={
								props.state.specific[props.currencyId].lang[
									props.langId
								].bannerLink
							}
						/>
						: null
				}
				<InputBox
					Title="Banner Pc Pic Url"
					ClassName="form-group"
					inputClass="form-control"
					OnChange={props.EditMainValue}
					dataColumn="bannerPCUrl"
					value={
						props.state.specific[props.currencyId].lang[
							props.langId
						].bannerPCUrl
					}
				/>
				<InputBox
					Title="Banner Mobile Pic Url"
					ClassName="form-group"
					inputClass="form-control"
					OnChange={props.EditMainValue}
					dataColumn="bannerMobileUrl"
					value={
						props.state.specific[props.currencyId].lang[
							props.langId
						].bannerMobileUrl
					}
				/>
			</div>
		</>
	);
};