import React from "react";
import moment from "moment";
import { defChannelType, defStatusType } from "./defCols";
const SpecialItemType = {
	actionBtn: "actionBtn"
};

const ChannelTypeFormat = type => {
	return defChannelType[type];
};

const StatusTypeFormat = type => {
	return defStatusType[type];
};

const PurposeFormat = purpose => {
	let result=purpose.split(",");
	let purposeString="";

	if(result.find(x=>x==1))
	{
		purposeString = purposeString + "Deposit,";
	}
	if(result.find(x=>x==2))
	{
		purposeString = purposeString + "Withdrawal,";
	}
	if(result.find(x=>x==3))
	{
		purposeString = purposeString + "Transfer,";
	}
	return purposeString.substr(0,purposeString.length-1);
};

const lastUpdateUserFormat = userName => {
	return userName ?? "-";
};

const dateFormat = (datetime)=>{
	return moment.parseZone(datetime).format("YYYY-MM-DD HH:mm:ss") ?? "-"  ;
}

const titleData = [
	{ 
		dataColumn: "siteName", 
		columnTitle: "Site" 
	},
	{ 
		dataColumn: "isoCode", 
		columnTitle: "Currency" 
	},
	{ 
		dataColumn: "bankCode", 
		columnTitle: "Bank" 
	},
	{ 
		dataColumn: "purpose", 
		columnTitle: "Purpose",
		body: { format: PurposeFormat } 
	},
	{
		dataColumn: "channel",
		columnTitle: "Channel",
		body: { format: ChannelTypeFormat }
	},
	{ 
		dataColumn: "agent", 
		columnTitle: "Agent" 
	},
	{ 
		dataColumn: "payName", 
		columnTitle: "pay" 
	},
	{ 
		dataColumn: "accountNo", 
		columnTitle: "Account No." 
	},
	{ 
		dataColumn: "accountName", 
		columnTitle: "Account Name" 
	},
	{ 
		dataColumn: "displayName", 
		columnTitle: "Display Name" 
	},
	{
		dataColumn: "status",
		columnTitle: "Status",
		body: { format: StatusTypeFormat }
	},
	{
		dataColumn: "createTime",
		columnTitle: "Create Time",
		body: { format: dateFormat }
	},
	{
		dataColumn: "creatorName",
		columnTitle: "Creator",
	},
	{
		dataColumn: "updateTime",
		columnTitle: "Last Update Time",
		body: { format: dateFormat }
	},
	{
		dataColumn: "updatorName",
		columnTitle: "Last Updator",
		body: { format: lastUpdateUserFormat }
	},
	{
		dataColumn: "edit",
		columnTitle: "Edit",
		body: { SpecialItem: SpecialItemType.actionBtn }
	}
];

//===============================================================================================================

export default ({
	GetSiteTransferAccountList,
	ClickCreateTransferAccount,
	ClickExportTransferAccount,
	ClickEdit,
	siteList,
	setSite,
	transferAccountList,
}) => {

	let siteRows = siteList.map((_data) => {
		return (
			<div key={`SiteOpt-${_data.siteId}`} className="custom-control custom-radio custom-control-inline">
				<input type="radio" id={`SiteId_${_data.siteId}`} name="SiteList" className="custom-control-input" value={_data.siteId} onChange={GetSiteTransferAccountList.bind(null, _data.siteId)} checked={_data.siteId === setSite} />
				<label className="custom-control-label" htmlFor={`SiteId_${_data.siteId}`}>{_data.siteName}</label>
			</div>
		)
	});

	let headerRows = [];
	titleData.forEach((e, i) => {
		headerRows.push(<th key={`title-${i}`}>{e.columnTitle}</th>);
	});

	let bodyRows = transferAccountList.map((_data, d_idx) => {
		let arr = titleData.map((element, t_idx) => {
			let body_content = null;
			if (element.body && element.body.SpecialItem) {
				if (element.body.SpecialItem === SpecialItemType.actionBtn) {
					body_content = (
						<ActionButtons clickEdit={ClickEdit.bind(null, d_idx)} />
					);
				}
			} else {
				body_content =
					element.body && element.body.format
						? element.body.format(_data[element.dataColumn])
						: _data[element.dataColumn];
			}
			return <td key={`td-${t_idx}`}>{body_content}</td>;
		});

		return <tr key={`body-${d_idx}`}>{arr}</tr>;
	});
	return (
		<div className="container-fluid">
			<h4 className="page-name">Transfer Account Management</h4>
			<div className="form-group">
				<label className="col-form-label d-block">Available Sites</label>
				{siteRows}
			</div>
			{
				setSite !=null && setSite >=0 ?
					<>
						<button
							className="btn btn-add"
							style={{ marginBottom: "20px" }}
							onClick={ClickCreateTransferAccount.bind(null, setSite)}
						>Create Account
						</button>
						<button
							className="btn btn-approved"
							style={{ marginBottom: "20px" }}
							onClick={ClickExportTransferAccount.bind(null, setSite)}
						>Export Excel
						</button>
						<div className="table">
							<table className="table table-striped">
								<thead>
									<tr>{headerRows}</tr>
								</thead>
								<tbody>{bodyRows}</tbody>
							</table>
						</div>
					</>
					: null
			}
		</div>
	);
};

const ActionButtons = props => {
	return (
		<div className="button-action" onClick={props.clickEdit}>
			<button className="btn btn-edit">
				<i className="icon-edit"></i>
			</button>
		</div>
	);
};
