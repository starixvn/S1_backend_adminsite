import React from "react";
import Pagination from './Pagination';
import moment from "moment-timezone";

const SpecialItemType = {
	actionBtn: "actionBtn"
};

export const dataColumnDefs = () => {
	return [
		{ dataColumn: "id", columnTitle: "Id" },
		{
			dataColumn: "siteName",
			columnTitle: "SiteName"
		},
		{
			dataColumn: "startTime",
			columnTitle: "StartTime",
			body: {
				format: dateFormat
			}
		},
		{
			dataColumn: "endTime",
			columnTitle: "EndTime",
			body: {
				format: dateFormat
			}
		},
		{
			dataColumn: "subject",
			columnTitle: "Subject",
			body: {
				attr: {
					class: "text-right"
				}
				// format:subjectFormat
			}
		},
		{ dataColumn: "content", columnTitle: "Content" },
		{ dataColumn: "creator", columnTitle: "Creator" },
		{
			dataColumn: "createTime",
			columnTitle: "CreateTime",
			body: {
				format: dateFormat
			}
		},
		{ dataColumn: "lastUpdater", columnTitle: "LastUpdater" },
		{
			dataColumn: "lastUpdateTime",
			columnTitle: "LastUpdateTime",
			body: {
				format: lastUpdateTime,
				subData: ["lastUpdater"]
			}
		},
		{
			dataColumn: "status",
			columnTitle: "Status",
			body: { format: boolFormat }
		},
		{
			dataColumn: "isTest",
			columnTitle: "isTest",
			body: { format: boolFormat }
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
							ClickEdit={props.ClickEdit.bind(null, d_idx)}
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
			<Pagination
				Data={{
					totalPages: props.PageInfo.totalPages,
					pageNumber: props.PageInfo.pageNumber,
					rowCount: props.PageInfo.rowCount
				}}
				ChangePage={props.ChangePage}
			/>
		</div>
	);
};

const ActionButtons = props => {
	return (
		<div className="button-action">
			<button className="btn btn-edit" onClick={props.ClickEdit}>
				<i className="icon-edit"></i>
			</button>
			<button className="btn btn-icon-delete" onClick={props.ClickDelete}>
				<i className="icon-delete"></i>
			</button>
		</div>
	);
};

const dateFormat = createTime => {
	return moment(createTime).format("l LTS");
};

const lastUpdateTime = (time, subData) => {
	return subData.lastUpdater ? dateFormat(time) : "-";
};

const boolFormat = value => {
	return value == 1 ? "True" : "False";
};

// const subjectFormat = (value)=>{
//     let displayCharsCount = 20;
//     return value.length <= displayCharsCount ? value : value.substring(0, displayCharsCount) + '...';
// }

// const contentFormat = (value)=>{
//     let displayCharsCount = 40;
//     return value.length <= displayCharsCount ? value : value.substring(0, displayCharsCount) + '...';
// }

export default Panel;
