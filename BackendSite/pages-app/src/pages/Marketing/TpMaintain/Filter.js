import React, { useState } from "react";
import moment from "moment";
import DateTimePicker from "react-datetime";

const filterdateformat = "YYYY-MM-DD";

const Panel = (props) => {

	let siteOptions = [];
	if (props.SiteList) {
		siteOptions = Object.keys(props.SiteList)
			.map(siteId => {
				return {
					value: Number(siteId),
					displayText: props.SiteList[siteId],
				};
			})
			.filter(item => {
				return item.value != 0;
			});
	}

	let TpOptions = [];
	if (props.tpList) {
		props.tpList.forEach((x) => {
			TpOptions.push({ value: x.id, displayText: x.tpName });
		});
	}

	let TpGameTypeOptions = [];
	if (props.typeList) {
		props.typeList.forEach((x) => {
			TpGameTypeOptions.push({ value: x.type, displayText: x.typeName });
		});
	}

	let TpCategoryOptions = [];
	if (props.tpCategoryList) {
		props.tpCategoryList.forEach((x) => {
			TpCategoryOptions.push({ value: x.categoryId, displayText: x.categoryName });
		});
	}

	return (
		<div className="filter">
			<div className="row-flex">
				{siteOptions.length > 0 ? (
					<CheckBoxElem
						Title="Site"
						Options={siteOptions}
						Selected={props.filterInfo.SelectSite}
						OnChange={props.onChangeCheckBox}
						OnChangeAll={props.onChangeAllCheckBox}
						dataColumn="SelectSite"
					/>
				) : null}

				{TpOptions.length > 0 ? (
					<DropDownBox
						columnTitle="Tp"
						options={TpOptions}
						dataColumn="SelectTpId"
					/>
				) : null}

				{TpGameTypeOptions.length > 0 ? (
					<DropDownBox
						columnTitle="GameType"
						options={TpGameTypeOptions}
						dataColumn="SelectGameType"
					/>
				) : null}

				{TpCategoryOptions.length > 0 ? (
					<DropDownBox
						columnTitle="Category"
						options={TpCategoryOptions}
						dataColumn="SelectCategory"
					/>
				) : null}

				<DateTimePickerElem
					columnTitle="From Date"
					dateTimePickerProps={{
						defaultValue: moment(props.filterInfo.StartDate).format(filterdateformat),
						timeFormat: false,
						onBlur: (e) => {
							try {
								props.onChange(moment(e).format(filterdateformat), "StartDate");
							} catch (ex) { }
						},
					}}
				/>

				<DateTimePickerElem
					columnTitle="To Date"
					dateTimePickerProps={{
						defaultValue: moment(props.filterInfo.EndDate).format(filterdateformat),
						timeFormat: false,
						onBlur: (e) => {
							try {
								props.onChange(moment(e).format(filterdateformat), "EndDate");
							} catch (ex) { }
						},
					}}
				/>

				<SwitchBtn
					columnTitle="IsExpired"
					dataColumn="SelectExpired"
				/>

				<div className="button-group">
					<button
						className="btn btn-search"
						type="submit"
						onClick={props.ClickSearch}
					>
						Search
					</button>
				</div>
			</div>
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

const CheckBoxElem = ({
	Title,
	Options,
	Selected = [],
	OnChange,
	OnChangeAll,
	dataColumn,
	mainKey,
}) => {
	let opt = Options.map((o) => {
		return (
			<CheckBox
				key={`${Title}-${o.value}${mainKey === null ? "" : `-${mainKey}`}`}
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
			<label className="label">{Title}</label>
			<CheckBox
				onChange={OnChangeAll}
				Title="ALL"
				Options={Options}
				checked={Selected.length === Options.length && Selected.length > 0}
				Value="ALL"
				dataColumn={dataColumn}
			/>
			{opt}
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
		<div className="input-item">
			<span className="label">{props.columnTitle}</span>
			<div className="field">
				<select onChange={onChange} value={props.value} >
					<option value="0" >ALL</option>
					{optionRows}
				</select>
			</div>
		</div>
	);
};

const SwitchBtn = props => {
	return (
		<div className="input-item">
			<span className="label">{props.columnTitle}</span>
			<input
				id={props.dataColumn}
				className="switch switch--flat"
				type="checkbox"
			/>
			<label htmlFor={props.dataColumn}></label>
		</div>
	);
};


export default Panel;
