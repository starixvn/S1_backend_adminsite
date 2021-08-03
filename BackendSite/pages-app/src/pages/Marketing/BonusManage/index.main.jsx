import React from "react";
import moment from "moment";
import { defWalletType, defPromotionType, defPromotionDeviceType } from "./defCols";
import PopBoxContent from "./Component/PopBoxContent";
import PopBoxContentforGen from "./Component/PopBoxContentforGen";
import { initPromotionType } from "./defCols";
import CheckBox from "./Component/Checkbox";
import MultiSelect from "react-multi-select-component";
import SelectBox from "./Component/SelectBox";
import { initStatusType,initDeviceType } from "./defCols";
// import DateTimePickerElem from "./Component/DateTimePicker";
import DateTimePicker from "react-datetime";

const SpecialItemType = {
	actionBtn: "actionBtn",
	statusBtn: "statusBtn",
	actionBtn2: "actionBtn2",
};

const promotionTypeFormat = type => {
	return defPromotionType[type];
};

const promotionDeviceTypeFormat = type => {
	return defPromotionDeviceType[type];
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

const walletTypeFormat = walletType => {
	return defWalletType[walletType]?.displayText;
};

const titleData = [
	{ dataColumn: "bonusCode", columnTitle: "BonusCode" },
	{ dataColumn: "siteName", columnTitle: "SiteName" },
	{
		dataColumn: "promotionType",
		columnTitle: "PromotionType",
		body: { format: promotionTypeFormat }
	},
	{
		dataColumn: "promotionDeviceType",
		columnTitle: "DeviceType",
		body: { format: promotionDeviceTypeFormat }
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
		dataColumn: "walletType",
		columnTitle: "WalletType",
		body: { format: walletTypeFormat }
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
	,
	{
		dataColumn: "ScratchCard",
		columnTitle: "ScratchCard",
		body: { SpecialItem: SpecialItemType.actionBtn2 }
	}
];
const DateTimePickerElem = (props) => {
	return (
	  <div className="input-item">
		<span className="label">{props.columnTitle}</span>
		<DateTimePicker {...props.dateTimePickerProps} />
	  </div>
	);
  };

  const InputField = (props) => {
	const onChange = (e) => {
	  if (props.onChange) {
		props.onChange(e.target.value, props.dataColumn);
	  }
	};
	return (
	  <div className="input-item">
		<span className="label">{props.columnTitle}</span>
		<div className="field">
		  <input
			type="text"
			defaultValue={props.defaultValue}
			onChange={onChange}
		  />
		</div>
	  </div>
	);
  };

export default ({
	ClickCreateBonus,
	ClickEdit,
	ClickEdit2,
	promotionList,
	UpdatePromotionStatus,
	SelectClick,
	Datas,
	NewDatas,
	Action,
	IsOpen,
	SiteList,
	StateFilter,
	SiteNameFilter,
	PromotionTypeFilter,
	StatusCheck,
	SetSiteNameFilter,
	SetPromotionTypeFilter,
	StateCheckBox,
	FilterOnchange
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
							clickClone={Action.ClickClone.bind(null, d_idx)}
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
				}else if (element.body.SpecialItem === SpecialItemType.actionBtn2) {
					if(_data.isSpecial)
					{
						body_content = (
							<ActionButtons2 
								clickEdit2={ClickEdit2.bind(null, d_idx)}
							/>
						);
					}
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
			<div className="filter">
				<div className="row-flex">
					<OptionElem
						columnTitle="SiteName"
						options={GetSelectedSiteName(SiteList)}
						onChange={SetSiteNameFilter}
						values={SiteNameFilter}
					/>
					<OptionElem
						columnTitle="PromotionType"
						options={GetSelectedPromotionType(initPromotionType)}
						onChange={SetPromotionTypeFilter}
						values={PromotionTypeFilter}
					/>
					<SelectBox
						Title="Status"
						Options={initStatusType}
						HandleChange={FilterOnchange}
						dataColumn="Status"
						SelectedValue={StateFilter.Status}
					/>
					<SelectBox
						Title="Device"
						Options={initDeviceType}
						HandleChange={FilterOnchange}
						dataColumn="Device"
						SelectedValue={StateFilter.Device}
					/>
				</div>
				<div className="row-flex">
					<DateTimePickerElem
						columnTitle="From"
						dateTimePickerProps={{
							value: StateFilter.StartDate,
							defaultValue: StateFilter.StartDate,
							timeFormat: false,
							onChange: e => {
								try {
									FilterOnchange(
										e,
										"StartDate"
									);
								} catch (ex) { }
							}
						}}
					/>
					<DateTimePickerElem
						columnTitle="To"
						dateTimePickerProps={{
							value: StateFilter.EndDate,
							defaultValue: StateFilter.EndDate,
							timeFormat: false,
							onChange: e => {
								try {
									FilterOnchange(
										e,
										"EndDate"
									);
								} catch (ex) { }
							}
						}}
					/>
					<InputField
                        columnTitle="Bonus Code"
                        defaultValue={StateFilter.BonusCode}
                        dataColumn="BonusCode"
                        onChange={FilterOnchange}
                    />
					<button
						className="btn btn-search"
						style={{ marginBottom: "10px" }}
						onClick={SelectClick}>
						Search
					</button>
				</div>
			</div>
			<button
				className="btn btn-add"
				style={{ marginBottom: "20px" }}
				onClick={ClickCreateBonus}>
				Create Bonus
			</button>
			<div className="table">
				<table className="table table-striped">
					<thead>
						<tr>{headerRows}</tr>
					</thead>
					<tbody>{bodyRows}</tbody>
				</table>
			</div>
			{
				IsOpen ?
					<PopBoxContent
						Datas={Datas}
						NewDatas={NewDatas}
						Action={Action}
						DefPromotionType={defPromotionType}
						DefWalletType={defWalletType}
					/>
					: null
			}
		</div>
	);
};

const ActionButtons = props => {
	return (
		<>
			<div className="button-action" onClick={props.clickEdit}>
				<button className="btn btn-edit">
					<i className="icon-edit"></i>
				</button>
			</div>
			<div className="button-action" onClick={props.clickClone}>
				<button className="btn btn-edit">
					<i className="fa fa-clone"></i>
				</button>
			</div>
		</>
	);
};
const ActionButtons2 = props => {
	return (
		<>
			<div className="button-action" onClick={props.clickEdit2}>
				<button className="btn btn-edit">
					<i className="icon-edit"></i>
				</button>
			</div>
		</>
	);
};
const GetSelectedSiteName = (props) => {
	let headerRows = [];
	props.map(x => {
		headerRows.push({
			label: x.siteName,
			value: x.siteId,
		})
	})
	return headerRows;
};
const GetSelectedPromotionType = (props) => {
	let headerRows = [];
	props.map(x => {
		headerRows.push({
			label: x.displayText,
			value: x.value,
		})
	})
	return headerRows;
};

const OptionElem = ({ columnTitle, options, onChange, values }) => {
	const OnSelect = (value) => {
	  onChange(value, "Currency");
	};
  
	return (
	  <div className="input-item" style={{ width: "200px", zIndex: "4" }}>
		<span className="label">{columnTitle}</span>
		<div className="field">
		  <MultiSelect
			disableSearch={"true"}
			options={options}
			value={values}
			onChange={OnSelect}
			labelledBy={"Select"}
			ItemRenderer={itemRenderer}
		  />
		</div>
	  </div>
	);
  };

  const itemRenderer = ({ checked, option, onClick }) => {
	return (
	  <div>
		<input
		  style={{ height: "15px", width: "20px" }}
		  id={option.label}
		  type="checkbox"
		  onChange={onClick}
		  checked={checked}
		/>
		<label htmlFor={option.label}>{option.label}</label>
	  </div>
	);
  };
