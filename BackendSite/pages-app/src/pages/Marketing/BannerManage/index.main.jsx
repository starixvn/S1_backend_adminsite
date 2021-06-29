import React from "react";
import moment from "moment-timezone";
import { defBannerType, defActionModeType, defShowStatusType,defStatusType } from "./defCols";
const SpecialItemType = {
	actionBtn: "actionBtn",
	statusBtn: "statusBtn"
};

const bannerTypeFormat = type => {
	return defBannerType[type];
};

const actionModeTypeFormat = type => {
	return defActionModeType[type];
};

const showStatusTypeFormat = type => {
	return defShowStatusType[type];
};

const StatusBtn = ({ checked, onChange, mainKey }) => {
	let id = `switch-${mainKey}`;
	return (
		<div className="switch__container">
			<input
				id={id}
				className="switch switch--flat"
				type="checkbox"
				checked={checked}
				onChange={onChange}
			/>
			<label htmlFor={id}></label>
		</div>
	);
};

const dateFormat = createTime => {
	return moment(createTime).format("l LTS");
};

const lastUpdateUserFormat = userName => {
	return userName ?? "-";
};

const lastUpdateTimeFormat = lastUpdTime => {
	return lastUpdTime ? moment(lastUpdTime).format("l LTS") : "-";
};

const titleData = [
	{ dataColumn: "bannerName", columnTitle: "BannerName" },
	{ dataColumn: "siteName", columnTitle: "SiteName" },
	{
		dataColumn: "bannerType",
		columnTitle: "BannerType",
		body: { format: bannerTypeFormat }
	},
	{
		dataColumn: "actionMode",
		columnTitle: "ActionMode",
		body: { format: actionModeTypeFormat }
	},
	{
		dataColumn: "showStatus",
		columnTitle: "ShowStatus",
		body: { format: showStatusTypeFormat }
	},
	{
		dataColumn: "startTime",
		columnTitle: "Start Time",
		body: { format: dateFormat }
	},
	{
		dataColumn: "endTime",
		columnTitle: "End Time",
		body: { format: dateFormat }
	},
	{
		dataColumn: "lastUpdateUser",
		columnTitle: "LastUpdateUser",
		body: { format: lastUpdateUserFormat }
	},
	{
		dataColumn: "lastUpdateTime",
		columnTitle: "LastUpdateTime",
		body: { format: lastUpdateTimeFormat }
	},
	{ dataColumn: "sort", columnTitle: "Sort" },
	{
		dataColumn: "status",
		columnTitle: "Status",
		body: { SpecialItem: SpecialItemType.statusBtn }
	},
	{
		dataColumn: "edit",
		columnTitle: "Edit",
		body: { SpecialItem: SpecialItemType.actionBtn }
	}
];



export default ({
	GetSiteBannerList,
	ClickCreateBanner,
	ClickEdit,
	UpdateBannerStatus,
	siteList,
	setSite,
	bannerList,
	FilterOnchange,
	BannerType,
	Status
}) => {
	let siteRows = siteList.map((_data) => {
		return (
			<div key={`SiteOpt-${_data.siteId}`} className="custom-control custom-radio custom-control-inline">
				<input type="radio" id={`SiteId_${_data.siteId}`} name="SiteList" className="custom-control-input" value={_data.siteId} onChange={GetSiteBannerList.bind(null, _data.siteId)} checked={_data.siteId === setSite} />
				<label className="custom-control-label" htmlFor={`SiteId_${_data.siteId}`}>{_data.siteName}</label>
			</div>
		)
	});

	let headerRows = [];
	titleData.forEach((e, i) => {
		headerRows.push(<th key={`title-${i}`}>{e.columnTitle}</th>);
	});
	let bodyRows = bannerList.map((_data, d_idx) => {
		let arr = titleData.map((element, t_idx) => {
			let body_content = null;
			if (element.body && element.body.SpecialItem) {
				if (element.body.SpecialItem === SpecialItemType.actionBtn) {
					body_content = (
						<ActionButtons clickEdit={ClickEdit.bind(null, d_idx)} />
					);
				} else if (element.body.SpecialItem === SpecialItemType.statusBtn) {
					body_content = (
						<StatusBtn
							mainKey={`${_data.siteId}-${_data.bannerName}`}
							checked={_data[element.dataColumn] === 1}
							onChange={UpdateBannerStatus.bind(null, d_idx)}
						/>
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
			<h4 className="page-name">Banner Management</h4>
			<div className="filter">
				<label className="col-form-label d-block">Available Sites</label>
				{siteRows}
				<div className="row-flex">
					<DropDownBox
					columnTitle="Banner Type"
					options={Object.keys(defBannerType).map((key) => {
						return { value: key, displayText: defBannerType[key] };
					})}
					hasAll={true}
					selectedValue={BannerType}
					dataColumn="BannerType"
					onChange={FilterOnchange}
					/>
					<DropDownBox
					columnTitle="Status"
					options={Object.keys(defStatusType).map((key) => {
						return { value: key, displayText: defStatusType[key] };
					})}
					hasAll={true}
					selectedValue={Status}
					dataColumn="Status"
					onChange={FilterOnchange}
					/>
				</div>
			</div>
			{
				setSite != "" ?
					<>
						<button
							className="btn btn-add"
							style={{ marginBottom: "20px" }}
							onClick={ClickCreateBanner}
						>Create Banner
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
