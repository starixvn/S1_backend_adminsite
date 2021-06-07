import React, { useState } from "react";
import DateTimePicker from "react-datetime";
import moment from "moment";

const elemType = {
	DateTimePicker: "DateTimePicker"
};
const dateformat = "YYYY-MM-DD HH:mm";

const commonColDefs = [
	{
		datacolumn: "beginDate",
		columnTitle: "BeginDate (UTC+0)",
		elemType: elemType.DateTimePicker
	},
	{
		datacolumn: "endDate",
		columnTitle: "EndDate (UTC+0)",
		elemType: elemType.DateTimePicker
	}
];

const EditPanel = props => {
	const [stateData, updateData] = useState({
		siteId: 0,
		id: 1,
		beginDate: moment().utc().format(dateformat),
		endDate: moment().utc().add(1,"day").format(dateformat),
		maintainType:"Date"
	  });

	const EditFiled = (v, datacolumn) => {
		updateData({
			...stateData,
			[datacolumn]: v
		});		
	};

	const InsertTpMaintain = () => {
		let msg = "";
		let check = true;
		if (!stateData.beginDate) {
			check = false;
			msg = "Please select beginDate.";
		} else if (!stateData.endDate) {
			check = false;
			msg = "Please select endDate.";
		}

		if (check) {
			props.InsertTpMaintain(stateData);
		} else {
			alert(msg);
		}
	};

	let commonRows = commonColDefs.map((col, cIdx) => {
		let elem = <></>;
		let key = `common-col-${cIdx}`;
		if (col.elemType === elemType.DateTimePicker) {
			elem = (
				<DateTimePickerElem
					key={key}
					columnTitle={col.columnTitle}					
					dateTimePickerProps={{
						dateFormat:"YYYY-MM-DD",
						timeFormat:"HH:mm",
						defaultValue: moment(stateData[col.datacolumn]).format(dateformat),
						onBlur: e => {
							try {
								EditFiled(
									moment(e).format(dateformat),
									col.datacolumn
								);
							} catch (ex) {}
						}
					}}
				/>
			);
		} 
		return elem;
	});

	return (
		<>
			<button
				className="btn btn-back"
				style={{ marginBottom: "20px" }}
				onClick={props.ClickBack}>
				Back
			</button>	

			<SiteList
				siteList={props.siteList}
				currentSelect={stateData.siteId}
				onChange={EditFiled}
				datacolumn={"siteId"}
			/>	

			<DropDownBox
				columnTitle="Tp Name"
				options={GetTPOptions(props.tpList)}
				selectedValue={props.id}
				dataColumn={"id"}
				onChange={EditFiled}
			/>  
			<br></br>
			{commonRows}
			<button
				className="btn btn-add"
				style={{
					marginTop: "20px",
					marginBottom: "40px",
					float: "right"
				}}
				onClick={InsertTpMaintain}> 
				Submit
			</button>
		</>
	);
};

const DateTimePickerElem = props => {
	return (
		<div className="form-group">
			<span className="label">{props.columnTitle}</span>
			<DateTimePicker
			 {...props.dateTimePickerProps} 
			 />
		</div>
	);
};

const SiteList = ({ siteList, onChange, currentSelect, datacolumn }) => {
	let radio =
		siteList &&
		Object.keys(siteList).map(siteId => {
			let siteName = siteList[siteId];
			return (
				<RadioItem
					key={`opt-site-${siteId}`}
					value={siteId}
					dispalyText={siteName}
					id={`rb-site-${siteId}`}
					onChange={onChange.bind(null, siteId, datacolumn)}
					checked={currentSelect == siteId}
				/>
			);
		});
	return (
		<div className="form-group">
			<label className="col-form-label d-block">Site Name</label>
			{radio}
		</div>
	);
};

const RadioItem = ({ value, dispalyText, id, onChange, checked }) => {
	return (
		<div className="custom-control custom-radio custom-control-inline">
			<input
				type="radio"
				id={id}
				name="SiteList"
				className="custom-control-input"
				value={value}
				onChange={onChange}
				checked={checked}
			/>
			<label className="custom-control-label" htmlFor={id}>
				{dispalyText}
			</label>
		</div>
	);
};

const GetTPOptions = (tpList) => {
	let TPOptions = [];	
	if (tpList != null) {
		tpList.forEach((x) => {
		TPOptions.push({ value: x.id, displayText: x.tpName });
	  });
	}
	return TPOptions;
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
		<div>
			<div className="input-item">
				<span className="label">{props.columnTitle}</span>
				<div className="field">
				<select onChange={onChange} value={props.value}>
					{optionRows}
				</select>
				</div>
			</div>
	  	</div>	
	);
  };
export default EditPanel;
