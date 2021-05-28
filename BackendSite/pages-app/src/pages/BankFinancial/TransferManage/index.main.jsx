import React from "react";
import moment from "moment";
import { defChannelType, defTransferType } from "./defCols";
import DateTimePicker from "react-datetime";
import {number_format, number_toFixed} from '../../../module/Formatting/NumberFormat';
const SpecialItemType = {
	actionBtn: "actionBtn"
};

const ChannelTypeFormat = type => {
	return defChannelType[type];
};

const TransferTypeFormat = type => {
	return defTransferType[type];
};

const lastUpdateUserFormat = userName => {
	return userName ?? "-";
};

const lastUpdateTimeFormat = lastUpdTime => {
	return lastUpdTime ?? "-";
};

const numberFormat = number => {
	return number_format(number);
};

const titleGroupData =[
	{ 
		columnTitle: "",
		colSpan:3
	},
	{ 
		columnTitle: "From Data",
		colSpan:12
	},
	{ 
		columnTitle: "To Data" ,
		colSpan:12
	},
	{ 
		columnTitle: "" ,
		colSpan:5
	}
];

const titleData = [
	{ 
		dataColumn: "transferId", 
		columnTitle: "TransferId" 
	},
	{ 
		dataColumn: "transferType", 
		columnTitle: "Transfer Type",
		body: { format: TransferTypeFormat } 
	},
	{ 
		dataColumn: "transDate", 
		columnTitle: "Trans Date" 
	},
	{ 
		dataColumn: "fromChannel", 
		columnTitle: "Channel",
		body: { format: ChannelTypeFormat }
	},
	{
		dataColumn: "fromAgent",
		columnTitle: "Agent"
	},
	{ 
		dataColumn: "fromPayName", 
		columnTitle: "Pay" 
	},
	{ 
		dataColumn: "fromSiteName", 
		columnTitle: "Site" 
	},
	{ 
		dataColumn: "fromCustId", 
		columnTitle: "CustId" 
	},
	{ 
		dataColumn: "fromUserName", 
		columnTitle: "User Name" 
	},
	{ 
		dataColumn: "fromIsoCode", 
		columnTitle: "Currency" 
	},
	{ 
		dataColumn: "fromBankCode", 
		columnTitle: "Bank" 
	},
	{
		dataColumn: "fromAccountNo",
		columnTitle: "Account No."
	},
	{
		dataColumn: "fromAccountDisplayName",
		columnTitle: "Account Display Name"
	},
	{
		dataColumn: "fromAmount",
		columnTitle: "Amount",
		body:{format:numberFormat}
	},
	{
		dataColumn: "fromFee",
		columnTitle: "Fee",
		body:{format:numberFormat}
	},
	{ 
		dataColumn: "toChannel", 
		columnTitle: "Channel",
		body: { format: ChannelTypeFormat }
	},
	{
		dataColumn: "toAgent",
		columnTitle: "Agent"
	},
	{ 
		dataColumn: "toPayName", 
		columnTitle: "Pay" 
	},
	{ 
		dataColumn: "toSiteName", 
		columnTitle: "Site" 
	},
	{ 
		dataColumn: "toCustId", 
		columnTitle: "CustId" 
	},
	{ 
		dataColumn: "toUserName", 
		columnTitle: "User Name" 
	},
	{ 
		dataColumn: "toIsoCode", 
		columnTitle: "Currency" 
	},
	{ 
		dataColumn: "toBankCode", 
		columnTitle: "Bank" 
	},
	{
		dataColumn: "toAccountNo",
		columnTitle: "Account No."
	},
	{
		dataColumn: "toAccountDisplayName",
		columnTitle: "Account Display Name"
	},
	{
		dataColumn: "toAmount",
		columnTitle: "Amount",
		body:{format:numberFormat}
	},
	{
		dataColumn: "toFee",
		columnTitle: "Fee",
		body:{format:numberFormat}
	},
	{
		dataColumn: "createTime",
		columnTitle: "Create Time"
	},
	{
		dataColumn: "creatorName",
		columnTitle: "Creator",
	},
	{
		dataColumn: "updateTime",
		columnTitle: "Last Update Time",
		body: { format: lastUpdateTimeFormat }
	},
	{
		dataColumn: "updatorName",
		columnTitle: "Last Updator",
		body: { format: lastUpdateUserFormat }
	},
	{
		dataColumn: "edit",
		columnTitle: "Action",
		body: { SpecialItem: SpecialItemType.actionBtn }
	}
];

//===============================================================================================================

export default ({
	filterInfo,
	onChange,
	ReasonList,
	Submit,
	setSite,

	ClickCreateTransferInfo,
	ClickExportTransferInfo,
	ClickEdit,
	ClickDelete,
	transferInfoList
}) => {
	let reasonListOptions = [];
	if (ReasonList) {
		reasonListOptions = ReasonList.map((i) => {
		  return {
			value: Number(i.reasonId),
			displayText: i.description,
		  };
		});
	  }

		let transferTypeList = Object.keys(defTransferType).map((key) => {
			return {
			  displayText: defTransferType[key],
			  value: key,
			};
		  });

	let headerRows = [];
	let groupHeaderRows=[];

    titleGroupData.forEach((e, i) => {
		groupHeaderRows.push(<th key={`group-${i}`} colSpan={e.colSpan}>{e.columnTitle}</th>);
	});

	titleData.forEach((e, i) => {
		headerRows.push(<th key={`title-${i}`}>{e.columnTitle}</th>);
	});

	let bodyRows = transferInfoList.map((_data, d_idx) => {
		let arr = titleData.map((element, t_idx) => {
			let body_content = null;
			if (element.body && element.body.SpecialItem) {
				if (element.body.SpecialItem === SpecialItemType.actionBtn) {
					body_content = (
						<div>
							<ActionButtons clickEdit={ClickEdit.bind(null, d_idx)} clickDelete={ClickDelete.bind(null, d_idx)}/>
							{/* <DeleteActionButtons clickDelete={ClickDelete.bind(null, d_idx)} /> */}
						</div>
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
			<h4 className="page-name">Transfer Info Management</h4>
			{/* <div className="form-group">
				<label className="col-form-label d-block">Available Sites</label>
				{siteRows}
			</div> */}
			<div className="filter">
				<div className="row-flex">
					<DropDownBox
          				columnTitle="Transfer Type"
          				options={transferTypeList}
          				hasAll={true}
         				selectedValue={filterInfo.TransferType}
          				dataColumn="TransferType"
          				onChange={(e) => onChange(e, "TransferType")}
        			/>
        			<DateTimePickerElem
          				columnTitle="From Date"
          				dateTimePickerProps={{
            				defaultValue: moment(filterInfo.FromDate).format("l"),
           					timeFormat: false,
            				onBlur: (e) => {
              				try {
                					onChange(moment(e).format("l"), "FromDate");
              					} catch (ex) {}
            				},
          				}}
        			/>
        			<DateTimePickerElem
          				columnTitle="To Date"
          				dateTimePickerProps={{
            				defaultValue: moment(filterInfo.ToDate).format("l"),
            				timeFormat: false,
            				onBlur: (e) => {
              				try {
                					onChange(moment(e).format("l"), "ToDate");
              					} catch (ex) {}
            				},
          				}}
        			/>
        			<DropDownBox
          				columnTitle="Reason"
          				options={reasonListOptions}
          				hasAll={true}
          				selectedValue={filterInfo.Reason}
          				dataColumn="Reason"
          				onChange={(e) => onChange(e, "Reason")}
        			/>
				</div>
				<div className="row-flex">
  					<InputField
            			columnTitle="From Account No"
            			Value={filterInfo.FromAccountNo}
            			dataColumn="FromAccountNo"
            			onChange={(e) => onChange(e, "FromAccountNo")}
            			defaultValue=""
          			/>
  					<InputField
          				columnTitle="From Account Display Name"
            			Value={filterInfo.FromAccountDisplayName}
            			dataColumn="FromAccountDisplayName"
            			onChange={(e) => onChange(e, "FromAccountDisplayName")}
            			defaultValue=""
          			/>
		  		</div>
		  		<div className="row-flex">
  					<InputField
            			columnTitle="To Account No"
            			Value={filterInfo.ToAccountNo}
            			dataColumn="ToAccountNo"
            			onChange={(e) => onChange(e, "ToAccountNo")}
            			defaultValue=""
          			/>
  					<InputField
            			columnTitle="To Account Display Name"
            			Value={filterInfo.ToAccountDisplayName}
            			dataColumn="ToAccountDisplayName"
            			onChange={(e) => onChange(e, "ToAccountDisplayName")}
            			defaultValue=""
          			/>
        			<div className="button-group">
          				<button className="btn btn-search" type="submit" onClick={Submit}>Submit</button>
        			</div>
				</div>
      		</div>
					<>
						<button
							className="btn btn-add"
							style={{ marginBottom: "20px" }}
							onClick={ClickCreateTransferInfo.bind(null, setSite)}
						>Create
						</button>
						<button
							className="btn btn-approved"
							style={{ marginBottom: "20px" }}
							onClick={ClickExportTransferInfo.bind(null, setSite)}
						>Export Excel
						</button>
						<div className="table">
							<table className="table table-striped">
								<thead>
									<tr>{groupHeaderRows}</tr>
									<tr>{headerRows}</tr>
								</thead>
								<tbody>{bodyRows}</tbody>
							</table>
						</div>
					</>
		</div>
	);
};

const ActionButtons = props => {
	return (
		<div className="button-action">
			<button className="btn btn-edit" onClick={props.clickEdit}>
				<i className="icon-edit"></i>
			</button>
			<button className="btn btn-delete" onClick={props.clickDelete}>
				<i className="icon-delete"></i>
			</button>
		</div>
	);
};

const DateTimePickerElem = (props) => {
	return (
	  <div className="input-item">
		<span className="label">{props.columnTitle}</span>
		<DateTimePicker {...props.dateTimePickerProps} />
	  </div>
	);
  };

  const DropDownBox = (props) => {
	const onChange = (e) => {
	  if (props.onChange) {
		props.onChange(e.target.value, props.dataColumn);
	  }
	};
	let optionRows = props.options.map((item) => {
	  return (
		<option key={`${props.ColumnTitle}-${item.value}`} value={item.value}>
		  {item.displayText}
		</option>
	  );
	});
  
	return (
	  <div className="input-item">
		<span className="label">{props.columnTitle}</span>
		<div className="field">
		  <select defaultValue={props.selectedValue} onChange={onChange}>
			{props.hasAll ? <option value="0">All</option> : null}
			{optionRows}
		  </select>
		</div>
	  </div>
	);
  };

  const InputField = (props) => {
	const EditField = (e) => {
	  if (props.onChange) {
		props.onChange(e.target.value, props.dataColumn);
	  } else if (props.onBlur) {
		props.onBlur(e.target.value, props.dataColumn);
	  }
	};
	return (
	  <div className="input-item">
		<span className="label">{props.columnTitle}</span>
		<input
		  className="form-control"
		  value={props.Value}
		  onChange={EditField}
		  onBlur={EditField}
		/>
	  </div>
	);
  };