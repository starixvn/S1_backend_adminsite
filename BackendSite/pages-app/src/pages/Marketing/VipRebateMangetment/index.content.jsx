import React from "react";
import moment from "moment";
import { initPeriodType, initWeekOptions, weekOptions } from "./defCols";
import RadioBox from "./Component/RadioBox";
import InputBox from "./Component/InputBox";
import SelectBox from "./Component/SelectBox";
import ActionButtons from "./Component/ActionButton";

export default ({
	state,
	action,
	processing,
	processArea,
}) => {
	let currencyTabArr = state.siteCurrencyList.map(item => {
		return (
			<button
				key={`${state.setSiteData}-currencyTab-${item}`}
				className={`btn btn-add ${state.currencyTab === item ? 'active1' : ''}`}
				onClick={action.GetRebateInfo.bind(this, item)}>
				{window.i18n[`lbl_Currency${item}`]}
			</button>
		);
	});
	let editStatus = state.initEdit["PeriodType"];
	let originalData = {
		"periodType": (state.setRebatePeriodData.periodType ? state.setRebatePeriodData.periodType : 0),
		"periodWeek": (state.setRebatePeriodData.periodWeek ? state.setRebatePeriodData.periodWeek : 0),
		"periodMonth": (state.setRebatePeriodData.periodMonth ? state.setRebatePeriodData.periodMonth : 0),
		"redeemHours": (state.setRebatePeriodData.redeemHours ? state.setRebatePeriodData.redeemHours : 0),
	}
	let data = editStatus === true ? state.setData : originalData;
	return (
		processArea == "RebatePeriod" ? null :
			<>
				<div className="filter">
					<RadioBox
						Title="Period Type"
						Options={initPeriodType}
						OnChange={action.SetData}
						dataColumn="periodType"
						Selected={data.periodType}
						disabled={processing !== true && editStatus === true ? false : true}
					/>
					<div className="row-flex">
						<label className="label">
							Pay Time {" (" + moment().format('Z') + ") "}
							<span className="bule-bold">{data.periodType === 2 ? weekOptions[data.periodWeek] : data.periodType === 3 ? `${data.periodMonth}th of every month` : ''}  {data.redeemHours === null || data.redeemHours === undefined || data.redeemHours === "" ? '' : `${data.redeemHours}:00`}</span>
						</label>
					</div>
					<div className="row-flex">
						{data.periodType === 2 ? (
							<SelectBox
								Title="Week Option"
								Options={initWeekOptions}
								HandleChange={action.SetData}
								dataColumn="periodWeek"
								SelectedValue={data.periodWeek}
								Disabled={processing !== true && editStatus === true ? false : true}
							/>
						) : data.periodType === 3 ?
							<InputBox
								Title="Date"
								OnChange={action.SetData}
								dataColumn="periodMonth"
								value={data.periodMonth}
								disabled={processing !== true && editStatus === true ? false : true}
							/>
							: null}
						<InputBox
							Title="Redeem Hours"
							OnChange={action.SetData}
							dataColumn="redeemHours"
							value={data.redeemHours}
							disabled={processing !== true && editStatus === true ? false : true}
						/>
					</div>
					{window.subFuncKeys && window.subFuncKeys.includes("f_vrm_edit") ? (
					<div className="row-flex">
						<ActionButtons
							isEdit={(editStatus === true) ? true : false}
							clickEdit={action.HandleEdit.bind(null, "PeriodType", "", originalData)}
							clickCancel={action.HandleEdit.bind(null, "PeriodType", "")}
							clickSubmit={action.SetRebatePeriod}
							disabled={processing}
						/>
					</div>
					) : null}
				</div>
				<div className="filter noBGcolor tag">{currencyTabArr}</div>
				{processArea == "RebateInfo" ? null : state.currencyTab !== 0 && state.setCurrencyData && Object.keys(state.setCurrencyData).length !== 0 ? (
					<div className="filter">
						<CategoryPanel
							state={state}
							displayText={
								window.i18n[
								`lbl_Currency${state.currencyTab}`
								]
							}
							currencyId={state.currencyTab}
							action={action}
							processArea={processArea}
						/>
					</div>
				) : null}
			</>
	);
};

const CategoryPanel = props => {
	let tpCategorytabArr = props.state.initTpCategoryList.map(item => {
		return (
			<button
				key={`${props.state.setSiteData}-tpCategoryTab-${item.categoryId}`}
				className={`btn btn-add ${props.state.tpCategoryTab === item.categoryId ? 'active1' : ''}`}
				onClick={props.action.GetTpGameTypeListbyCategory.bind(this, item.categoryId)}
			>
				{item.categoryName}
			</button>
		);
	});
	return (
		<>
			<div className="filter noBGcolor tag">{tpCategorytabArr}</div>
			{props.state.tpCategoryTab !== 0 ?
				<CategoryDataPanel
					state={props.state}
					action={props.action}
					processArea={props.processArea}
				/> : null
			}
		</>
	);
};

const CategoryDataPanel = props => {
	let vipListDataArr = props.state.siteVipList.map(item => {
		let editStatus = props.state.initEdit["categoryDataLevel" + item.level];
		let originalData = {
			"categoryDataLevel": props.state.setCategoryData.categoryLevelList && Object.keys(props.state.setCategoryData.categoryLevelList).length !== 0 ?
				props.state.setCategoryData.categoryLevelList.find(s => s.level === item.level).percentage : "No Setting"
		}
		let value = editStatus === true ? props.state.setData.categoryDataLevel : originalData.categoryDataLevel;
		return (
			<div
				className="row-flex"
				key={`${props.state.setSiteData}-categoryDataLevel-${item.level}`}
			>
				<InputBox
					Title={item.levelName}
					OnChange={props.action.SetData}
					dataColumn="categoryDataLevel"
					value={value ? value : 0}
					disabled={props.processing !== true && editStatus === true ? false : true}
				/>
				<div style={{ width: "25px", height: "25px" }}>%</div>
				{window.subFuncKeys && window.subFuncKeys.includes("f_vrm_edit") ?
				<ActionButtons
					isEdit={editStatus === true ? true : false}
					clickEdit={props.action.HandleEdit.bind(null, "categoryDataLevel", item.level, originalData)}
					clickCancel={props.action.HandleEdit.bind(null, "categoryDataLevel", item.level)}
					clickSubmit={props.action.SetRebateCategory.bind(null, item.level)}
					disabled={props.processing}
				/>
				: null}
			</div>
		);
	});
	let editStatus = props.state.initEdit["Payout"];
	let originalData = {
		"minPayout": props.state.setCategoryPayoutData && Object.keys(props.state.setCategoryPayoutData).length !== 0 ? props.state.setCategoryPayoutData.minPayout : "No Setting",
		"maxPayout": props.state.setCategoryPayoutData && Object.keys(props.state.setCategoryPayoutData).length !== 0 ? props.state.setCategoryPayoutData.maxPayout : "No Setting"
	}
	let data = editStatus === true ? props.state.setData : originalData;
	return (
		<>
			<div className="filter">
				<label className="">
					<span className="text">Payout Setting</span>
				</label>
				<div className="row-flex">
					<InputBox
						Title="Min Payout"
						OnChange={props.action.SetData}
						dataColumn="minPayout"
						value={data ? data.minPayout : data.minPayout === 0 ? "No Limit" : null}
						disabled={props.processing !== true && editStatus === true ? false : true}
					/>
					<InputBox
						Title="Max Payout"
						OnChange={props.action.SetData}
						dataColumn="maxPayout"
						value={data ? data.maxPayout : data.maxPayout === 0 ? "No Limit" : null}
						disabled={props.processing !== true && editStatus === true ? false : true}
					/>
					{window.subFuncKeys && window.subFuncKeys.includes("f_vrm_edit") ?
					<ActionButtons
						isEdit={editStatus === true ? true : false}
						clickEdit={props.action.HandleEdit.bind(null, "Payout", "", originalData)}
						clickCancel={props.action.HandleEdit.bind(null, "Payout", "")}
						clickSubmit={props.action.SetRebatePayoutLimit}
						disabled={props.processing}
					/>
					: null}
				</div>
			</div>
			<div className="filter">
				<label className="">
					<span className="text">Vip Setting</span>
				</label>
				{vipListDataArr}
			</div>
			{props.processArea == "TpGameTypeList" ? null : props.state.tpCategoryTab !== 0 && props.state.tpGameTypeListbyCategory && Object.keys(props.state.tpGameTypeListbyCategory).length !== 0 ? (
				<div className="filter">
					<label className="">
						<span className="text">Game Type</span>
					</label>
					<TpGameTypePanel
						state={props.state}
						currencyId={props.state.currencyTab}
						tpCategoryId={props.state.tpCategoryTab}
						action={props.action}
						processArea={props.processArea}
					/>
				</div>
			) : null}
		</>
	);
};

const TpGameTypePanel = props => {
	let tpGameTypeListtabArr = props.state.tpGameTypeListbyCategory.map(item => {
		return (
			<button
				key={`${props.state.setSiteData}-tpGameTypeTab-${item.type}`}
				className={`btn btn-add ${props.state.tpGameTypeTab === item.type ? 'active1' : ''}`}
				onClick={props.action.GetRebateInfoByGameType.bind(this, props.currencyId, item.type)}
			>
				{item.typeName}
			</button>
		);
	});
	return (
		<>
			<div className="filter noBGcolor tag">{tpGameTypeListtabArr}</div>
			{props.processArea == "RebateInfoByGameType" ? null : props.state.tpGameTypeTab !== 0 ? (
				<TpGameTypeDataPanel
					state={props.state}
					action={props.action}
				/>
			) : null
			}
		</>
	);
};

const TpGameTypeDataPanel = props => {
	let vipListDataArr = props.state.siteVipList.map(item => {
		let editStatus = props.state.initEdit["tpGameTypeLevel" + item.level];
		let originalData = {
			"tpGameTypeLevel": props.state.setGameTypeData && Object.keys(props.state.setGameTypeData).length !== 0 ?
				props.state.setGameTypeData.find(s => s.level === item.level).percentage : "Base on category"
		}
		let value = editStatus === true ? props.state.setData.tpGameTypeLevel : originalData.tpGameTypeLevel;
		return (
			<div
				className="row-flex"
				key={`${props.state.setSiteData}-tpGameTypeLevel-${item.level}`}
			>
				<InputBox
					Title={item.levelName}
					OnChange={props.action.SetData}
					dataColumn="tpGameTypeLevel"
					value={value ? value : 0}
					disabled={props.processing !== true && editStatus === true ? false : true}
				/>
				<div style={{ width: "25px", height: "25px" }}>%</div>
				{window.subFuncKeys && window.subFuncKeys.includes("f_vrm_edit") ?
				<ActionButtons
					isEdit={editStatus === true ? true : false}
					clickEdit={props.action.HandleEdit.bind(null, "tpGameTypeLevel", item.level, originalData)}
					clickCancel={props.action.HandleEdit.bind(null, "tpGameTypeLevel", item.level)}
					clickSubmit={props.action.SetRebateGameType.bind(null, item.level)}
					disabled={props.processing}
				/>
				: null}
			</div>
		);
	});
	return (
		<div className="filter">
			<label className="">
				<span className="text">Vip Setting</span>
			</label>
			{vipListDataArr}
		</div>
	);
};