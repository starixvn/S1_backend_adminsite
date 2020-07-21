import React from "react";
import moment from "moment";

const SpecialItemType = {
	actionBtn: "actionBtn",
	statusBtn: "statusBtn"
};

const defPromotionType = {
	1: "First Deposit",
	2: "Re-Deposit",
	3: "Rebate",
	4: "Special"
};

const promotionTypeFormat = type => {
	return defPromotionType[type];
};

const StatusBtn = ({ checked, onChange, mainKey }) => {
	let id = `switch-${mainKey}`
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

const titleData = [
	{ dataColumn: "bonusCode", columnTitle: "BonusCode" },
	{ dataColumn: "siteName", columnTitle: "SiteName" },
	{
		dataColumn: "promotionType",
		columnTitle: "PromotionType",
		body: { format: promotionTypeFormat }
	},
	{ dataColumn: "description", columnTitle: "Description" },
	{
		dataColumn: "periodFrom",
		columnTitle: "PeriodFrom",
		body: { format: dateFormat }
	},
	{
		dataColumn: "periodTo",
		columnTitle: "PeriodTo",
		body: { format: dateFormat }
	},
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
	ClickCreateBonus,
	ClickEdit,
	promotionList,
	UpdatePromotionStatus
}) => {
	let headerRows = [];
	titleData.forEach((e, i) => {
		headerRows.push(<th key={`title-${i}`}>{e.columnTitle}</th>);
	});

	let bodyRows = promotionList.map((_data, d_idx) => {
		let arr = titleData.map((element, t_idx) => {
			let body_content = null;
			if (element.body && element.body.SpecialItem) {
				if (element.body.SpecialItem === SpecialItemType.actionBtn) {
					body_content = (
						<ActionButtons
							clickEdit={ClickEdit.bind(null, d_idx)}
						/>
					);
				} else if (
					element.body.SpecialItem === SpecialItemType.statusBtn
				) {
					body_content = (
						<StatusBtn
							mainKey={`${_data.siteId}-${_data.bonusCode}`}
							checked={_data[element.dataColumn] === 1}
							onChange={UpdatePromotionStatus.bind(this, d_idx)}
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
			<h4 className="page-name">Bonus Management</h4>
			<button className="btn btn-add" style={{"marginBottom": "20px"}} onClick={ClickCreateBonus}>Create Bonus</button>
			<div className="table">
				<table className="table table-striped">
					<thead>
						<tr>{headerRows}</tr>
					</thead>
					<tbody>{bodyRows}</tbody>
				</table>
			</div>
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
