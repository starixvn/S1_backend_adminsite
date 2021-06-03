import React from "react";
import moment from "moment";

const SpecialItemType = {
	actionBtn: "actionBtn"
};

export const dataColumnDefs = () => {
	return [
		{ 
			dataColumn: "sn", 
			columnTitle: "SN" 
		},
		{
			dataColumn: "siteId",
			columnTitle: "SiteName"
		},
		{
			dataColumn: "tpId",
			columnTitle: "TpName"
		},
		{
			dataColumn: "beginDate",
			columnTitle: "BeginDate (UTC+0)",
		},
		{
			dataColumn: "endDate",
			columnTitle: "EndDate (UTC+0)",
		},
		{
			columnTitle: "Action",
			header: {
				attr: {
					className: "width-85"
				}
			},
			body: {
				SpecialItem: SpecialItemType.actionBtn
			}
		}
	];
};

const Panel = props => {
	const columnDefs = dataColumnDefs();
	let headerRows = [];

	columnDefs.forEach((element, idx) => {
		headerRows.push(
			<th
				key={`th-${element.dataColumn}-${idx}`}
				scope="col"
				className={
					element.header &&
					element.header.attr &&
					element.header.attr.class
						? element.header.attr.class
						: ""
				}>
				{element.columnTitle}
			</th>
		);
	});

	let bodyRows = props.Datas.map((data, d_idx) => {
		let row = columnDefs.map((element, c_idx) => {
			let body_content = null;
			if (element.body && element.body.SpecialItem) {
				if (element.body.SpecialItem === SpecialItemType.actionBtn) {
					body_content = (
						<ActionButtons
							ClickDelete={props.ClickDelete.bind(null, d_idx)}
						/>
					);
				}
			} else {
				let subData = {};
				if (element.body && element.body.subData) {
					element.body.subData.forEach(s => {
						subData[s] = data[s];
					});
				}

				body_content =
					element.body && element.body.format
						? element.body.format(data[element.dataColumn], subData)
						: data[element.dataColumn];
			}

			return (
				<td
					key={`td-${element.dataColumn}-${c_idx}`}
					className={
						element.body &&
						element.body.attr &&
						element.body.attr.class
							? element.body.attr.class
							: ""
					}>
					{body_content}
				</td>
			);
		});

		return <tr key={`result-body-${d_idx}`}>{row}</tr>;
	});

	return (
		<div className="table">
			<table className="table table-striped">
				<thead>
					<tr>{headerRows}</tr>
				</thead>
				<tbody>{bodyRows}</tbody>
			</table>
		</div>
	);
};

const ActionButtons = props => {
	return (
		<div className="button-action">
			<button className="btn btn-icon-delete" onClick={props.ClickDelete}>
				<i className="icon-delete"></i>
			</button>
		</div>
	);
};

export default Panel;
