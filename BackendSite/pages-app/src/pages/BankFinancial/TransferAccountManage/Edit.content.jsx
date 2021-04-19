import React from "react";
import InputBox from "./Component/InputBox";
import CheckBox from "./Component/Checkbox";
import SelectBox from "./Component/SelectBox";
import RadioBox from "./Component/RadioBox";
import TextBox from "./Component/TextBox";
import moment from "moment";
import { initChannelType,initSiteId, initStatusType,initPurposeType } from "./defCols";

export default ({ state, action, processing,isAdd }) => {	
	const currencyList = state.currencyList;
	const currencyOptions =[];
	currencyOptions.push({value:0,displayText:" - Select Currency -"});
	currencyList.map(i => {
		currencyOptions.push({
			value: Number(i),
			displayText: window.i18n[`lbl_Currency${i}`]
		});
	});
	return (
		<div className="container-fluid" id="Panel">
			<div className="filter noBGcolor"></div>
			<div className="panel level-1 mt-20">
				<div
					className="panel-title no-arrow"
					style={{ justifyContent: "space-between" }}>
					<span style={{ color: "white" }}>Transfer Account Info</span>
				</div>
			</div>
			<div id="searchForm">
				<div className="filter">
					<div className="row-flex">
						<SelectBox
							Title="Site"
							Options={initSiteId}
							HandleChange={action.EditSiteIdSelectBox}
							dataColumn="siteId"
							SelectedValue={state.siteId}
							//Disabled={!isAdd}
							Disabled={true}
						/>
						<InputBox
							Title="SiteName"
							OnChange={action.EditMainValue}
							dataColumn="siteName"
							value={state.siteName}
							Disabled={true}
						/>
					</div>
					<div className="row-flex">
						<SelectBox
							Title="Currency"
							Options={currencyOptions}
							HandleChange={action.EditCurrencySelectBox}
							dataColumn="currencyId"
							SelectedValue={state.currencyId}
							mainKey={state.siteId}
						/>
						<SelectBox
							Title="Bank Code"
							Options={state.setBankCodeList}
							HandleChange={action.EditMainValue}
							dataColumn="sysId"
							SelectedValue={state.sysId}
						/>
					</div>
					<div className="row-flex">
						<CheckBox
							Options={initPurposeType}
							Title="Purpose"
							OnChange={action.EditPurposeCheckBox}
							dataColumn="purpose"
							Selected={state.purpose}
						/>
					</div>
					<div className="row-flex">
						<SelectBox
							Title="Channel"
							Options={initChannelType}
							HandleChange={action.EditChannelSelectBox}
							dataColumn="channel"
							SelectedValue={state.channel}
						/>
					</div>
					<div className="row-flex">
						{state.channel == 3 ?
						<>
						<SelectBox
							Title="Pay"
							Options={state.payList}
							HandleChange={action.EditMainValue}
							dataColumn="payId"
							SelectedValue={state.payId}
						/>
						</>
						:
						<>
						<InputBox
							Title="Agent"
							OnChange={action.EditMainValue}
							dataColumn="agent"
							value={state.agent}
						/>
						</>
						}
					</div>
					<div className="row-flex">
						<InputBox
							Title="Account No."
							OnChange={action.EditMainValue}
							dataColumn="accountNo"
							value={state.accountNo}
							Disabled={!isAdd}
						/>
						<InputBox
							Title="Account Name"
							OnChange={action.EditMainValue}
							dataColumn="accountName"
							value={state.accountName}
						/>
					</div>
					<div className="row-flex">
						<InputBox
							Title="Display Name"
							OnChange={action.EditMainValue}
							dataColumn="displayName"
							value={state.displayName}
						/>
					</div>
					<div className="row-flex">
						<InputBox
							Title="Initial Amount"
							OnChange={action.EditMainValue}
							dataColumn="initialAmount"
							value={state.initialAmount}
							type="number"
						/>
						<InputBox
							Title="Max Balance Alert Amount"
							OnChange={action.EditMainValue}
							dataColumn="maxBalanceAlertAmount"
							value={state.maxBalanceAlertAmount}
							type="number"
						/>
					</div>
					<div className="row-flex">
						<SelectBox
							Title="Reason"
							Options={state.reasonList}
							HandleChange={action.EditMainValue}
							dataColumn="reason"
							SelectedValue={state.reason}
						/>
					</div>
					<div className="row-flex">
						<TextBox
							Title="Remark"
							OnChange={action.EditMainValue}
							dataColumn="remark"
							value={state.remark}
						/>
					</div>
					<div className="row-flex">
						<RadioBox
							Title="Status"
							Options={initStatusType}
							OnChange={action.EditMainValue}
							dataColumn="status"
							Selected={state.status}
						/>
					</div>
					<hr></hr>
					<div className="row-flex">
						<InputBox
							Title="Create Time"
							OnChange={action.EditMainValue}
							dataColumn="createTime"
							value={state.createTime}
							Disabled={true}
						/>
						<InputBox
							Title="Creator"
							OnChange={action.EditMainValue}
							dataColumn="creatorName"
							value={state.creatorName}
							Disabled={true}
						/>
					</div>
					<div className="row-flex">
						<InputBox
							Title="Update Time"
							OnChange={action.EditMainValue}
							dataColumn="updateTime"
							value={state.updateTime}
							Disabled={true}
						/>
						<InputBox
							Title="Updator"
							OnChange={action.EditMainValue}
							dataColumn="updatorName"
							value={state.updatorName}
							Disabled={true}
						/>
					</div>
					<div className="row-flex">
						<InputBox
							Title="Id"
							OnChange={action.EditMainValue}
							dataColumn="id"
							value={state.id}
							Disabled={true}
						/>
					</div>
				</div>
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
