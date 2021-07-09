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
		columnTitle: "BeginDate Time" + " (" + moment().format('Z') + ")",
		elemType: elemType.DateTimePicker
	},
	{
		datacolumn: "endDate",
		columnTitle: "EndDate Time" + " (" + moment().format('Z') + ")",
		elemType: elemType.DateTimePicker
	}
];

const EditPanel = props => {
	const [stateData, updateData] = useState({
		siteId: 0,
		id: 1,
		beginDate: moment().format(dateformat),
		endDate: moment().add(1,"day").format(dateformat),
		maintainType:"Date",
		typeList: props.typeList,
		tpGameType: [],
	});

	const EditFiled = (v, datacolumn) => {
		updateData({
			...stateData,
			[datacolumn]: v
		});	
		if (datacolumn == "id"){
			GetTpGameTypeOptions(stateData.typeList, v);
		}			
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


	const CheckBox_OnChange = (v, dataColumn) => {
		console.log("OnChange");
		let isRemove = stateData[dataColumn].includes(v);
		let newChecked = isRemove
		  ? stateData[dataColumn].filter((value) => value !== v)
		  : stateData[dataColumn].push(v);
		let currentTab = stateData[dataColumn];
		if (newChecked.length === 0) {
		  currentTab = [];
		} else if (currentTab === null) {
		  currentTab = newChecked;
		}
	
		if (isRemove && !newChecked.includes(currentTab)) {
		  currentTab = newChecked.length === 0 ? [] : newChecked;
		}
		console.log(currentTab);
		updateData({
		  ...stateData,
		  [dataColumn]: currentTab,
		});
	};
	
	const CheckBox_CheckAll = (v, dataColumn, options) => {
		console.log("CheckAll");
		if (v !== "All") return false;
		let currentTab =
			stateData[dataColumn].length === options.length
			? []
			: options.map((o) => {
				return o.value;
			  });
		console.log(currentTab);
		updateData({
		  ...stateData,
		  [dataColumn]: currentTab,
		});
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
			 <div className="row-flex">
				{stateData.typeList ? (
				<CheckBoxElem
					Title="Tp GameType"
					Options={GetTpGameTypeOptions(stateData.typeList, stateData.id)}
					Selected={stateData.tpGameType}
					OnChange={CheckBox_OnChange}
					OnChangeAll={CheckBox_CheckAll}
					dataColumn="tpGameType"
				/>
				) : null}
			</div> 
			
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

const GetTpGameTypeOptions = (typeList, tpId) => {
	let TpGameTypeOptions = [];	
	if (typeList != null) {
		typeList.forEach((x) => {
			if	(tpId == x.tpId){
				TpGameTypeOptions.push({ value: `${x.type}-${x.categoryId}`, displayText: x.typeName });
			}			
	  });
	}
	return TpGameTypeOptions;		
};  

const CheckBoxElem = ({
	Title,
	Options,
	Selected = [],
	OnChange,
	OnChangeAll,
	dataColumn,
  }) => {
	let opt = Options.map((o) => {
	  return (
		<CheckBox
		  key={o.value}
		  Options={null}
		  onChange={OnChange}
		  Title={o.displayText}
		  checked={Selected.includes(o.value)}
		  Value={o.value}
		  dataColumn={dataColumn}
		/>
	  );
	});
	return (
	  <div className="input-item" style={{ width: "auto" }}>
		<span className="label">{Title}</span>
		<div className="field">						
			<CheckBox
			onChange={OnChangeAll}
			Title="All"
			Options={Options}
			checked={true}
			Value="All"
			dataColumn={dataColumn}
			/>
			{opt}
		</div>
	  </div>
	);
};
  
const CheckBox = (props) => {
	const OnChange = (e) => {
	  props.onChange(props.Value, props.dataColumn, props.Options);
	};
	return (
	  <label className="checkbox checkbox-checked">
		<input
		  type="checkbox"
		  style={{ width: "0" }}
		  value={props.Value}
		  checked={props.checked}
		  onChange={OnChange}
		/>
		<i className="icon icon-checkbox"></i>
		<span className="text">{props.Title}</span>
	  </label>
	);
};
export default EditPanel;
