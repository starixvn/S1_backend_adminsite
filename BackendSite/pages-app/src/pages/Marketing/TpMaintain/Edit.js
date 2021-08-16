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
		siteId: props.IsAdd ? 0 : props.Datas.siteId,
		tpId: props.IsAdd ? 1 : props.Datas.tpId,
		beginDate: props.IsAdd ? moment().format(dateformat) : UtcToLocal(props.Datas.beginDate),
		endDate: props.IsAdd ? moment().add(1, "day").format(dateformat) : UtcToLocal(props.Datas.endDate),
		maintainType: props.IsAdd ? "Date" : props.Datas.maintainType,
		typeList: props.typeList,
		sn: props.IsAdd ? null : props.Datas.sn,
		tpGameType: props.IsAdd ? null : props.Datas.tpGameType,
	});

	React.useEffect(() => {
		if (!props.IsAdd) {
			$("input[type='checkbox']").each(function () {
				$(this).prop('disabled', true);
				if ($(this).prop('id') == stateData.tpGameType) {
					$(this).prop("checked", true)
				}
			});

			if ($("input[type='checkbox'][value='0']").prop("checked")) {
				$("#CheckItemGroup").hide();
			} else {
				$("#CheckItemGroup").show();
			}
		}
	});

	const EditFiled = (v, datacolumn) => {
		updateData({
			...stateData,
			[datacolumn]: v
		});
		if (datacolumn == "tpId") {
			$("input[type='checkbox']").each(function () {
				$(this).prop("checked", false);
			});
			$("#CheckItemGroup").show();
			GetTpGameTypeOptions(stateData.typeList, v);
		}
	};

	const SetTpMaintain = () => {
		let tpGameTypeList = "";
		if ($("input[type='checkbox'][value='0']").prop("checked")) {
			tpGameTypeList = "0";
		} else {
			$("input[type='checkbox'][value!='0']:checked").each(function () {
				tpGameTypeList += `${$(this).val()},`;
			});
		}

		let msg = "";
		let check = true;
		if (!stateData.beginDate) {
			check = false;
			msg = "Please select beginDate.";
		} else if (!stateData.endDate) {
			check = false;
			msg = "Please select endDate.";
		}
		else if (tpGameTypeList == "") {
			check = false;
			msg = "Please select tpGametype.";
		}

		if (check) {
			props.SetTpMaintain(stateData, tpGameTypeList, props.IsAdd);
		} else {
			alert(msg);
		}
	};

	const CheckBox_OnChange = (v, dataColumn) => {
	};

	const CheckBox_OnChangeAll = (v, dataColumn) => {
		$("input[type='checkbox'][value!='0']").each(function () {
			$(this).prop("checked", false);
		});
		if ($("input[type='checkbox'][value='0']").prop("checked")) {
			$("#CheckItemGroup").hide();
		} else {
			$("#CheckItemGroup").show();
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
						dateFormat: "YYYY-MM-DD",
						timeFormat: "HH:mm",
						defaultValue: moment(stateData[col.datacolumn]).format(dateformat),
						onBlur: e => {
							try {
								EditFiled(
									moment(e).format(dateformat),
									col.datacolumn
								);
							} catch (ex) { }
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
				datacolumn="siteId"
				IsAdd={props.IsAdd}
			/>

			<DropDownBox
				columnTitle="Tp Name"
				options={GetTPOptions(props.tpList)}
				currentSelect={stateData.tpId}
				dataColumn="tpId"
				onChange={EditFiled}
				IsAdd={props.IsAdd}
			/>
			<br></br>
			<div className="row-flex">
				{stateData.typeList ? (
					<CheckBoxElem
						Title="Tp GameType"
						Options={GetTpGameTypeOptions(stateData.typeList, stateData.tpId)}
						OnChange={CheckBox_OnChange}
						OnChangeAll={CheckBox_OnChangeAll}
						dataColumn="tpGameType"
						currentSelect={stateData.tpGameType}
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
				onClick={SetTpMaintain}>
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

const SiteList = ({ siteList, onChange, currentSelect, datacolumn, IsAdd }) => {
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
					IsAdd={IsAdd}
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

const RadioItem = ({ value, dispalyText, id, onChange, checked, IsAdd }) => {
	return (
		<div className="custom-control custom-radio custom-control-inline">
			<input disabled={IsAdd ? false : true}
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
			<option key={`${props.ColumnTitle}-${item.value}`} value={item.value} selected={props.currentSelect == item.value}>
				{item.displayText}
			</option>
		);
	});
	return (
		<div>
			<div className="input-item">
				<span className="label">{props.columnTitle}</span>
				<div className="field">
					<select onChange={onChange} value={props.value} disabled={props.IsAdd ? false : true}>
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
			if (tpId == x.tpId) {
				TpGameTypeOptions.push({ value: `${x.type}-${x.categoryId}`, displayText: x.typeName, id: x.type });
			}
		});
	}
	return TpGameTypeOptions;
};

const CheckBoxElem = ({
	Title,
	Options,
	OnChange,
	OnChangeAll,
	dataColumn
}) => {
	let opt = Options.map((o) => {
		return (
			<CheckBox
				onChange={OnChange}
				Options={null}
				Title={o.displayText}
				Value={o.value}
				dataColumn={dataColumn}
				id={o.id}
			/>
		);
	});
	return (
		<div className="input-item" style={{ width: "auto" }} >
			<span className="label">{Title}</span>
			<div className="field">
				<CheckBox
					onChange={OnChangeAll}
					Title="All"
					Options={Options}
					Value="0"
					dataColumn={dataColumn}
					id="0"
				/>
				<br />
				<div id="CheckItemGroup">
					{opt}
				</div>
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
				onChange={OnChange}
				id={props.id}
			/>
			<i className="icon icon-checkbox"></i>
			<span className="text">{props.Title}</span>
		</label>
	);
};

const UtcToLocal = (time) => {
	return moment.utc(time).local().format('YYYY-MM-DD HH:mm');
}
export default EditPanel;
