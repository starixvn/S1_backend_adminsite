import React, { useState } from "react";
import DateTimePicker from "react-datetime";
import moment from "moment-timezone";

const allSiteDisplayText = "All";

const elemType = {
	SwitchBtn: "SwitchBtn",
	DateTimePicker: "DateTimePicker"
};



const EditPanel = props => {
	const [stateData, updateData] = useState({
		...props.Datas,
		subject: props.IsAdd ? {} : JSON.parse(props.Datas.subject),
		content: props.IsAdd ? {} : JSON.parse(props.Datas.content)
	});
	const [langDatas, setlangDatas] = useState(props.IsAdd ? null : props.siteList[props.Datas.siteId == 0?1:props.Datas.siteId].iLangList);
    let allLang = ["en", "cs", "ch", "th", "id", "vn"];
	const EditFiled = (v, datacolumn, subDatas) => {
		if (subDatas && subDatas.lang) {
			updateData({
				...stateData,
				[datacolumn]: {
					...stateData[datacolumn],
					[subDatas.lang]: v
				}
			});
		} else if (datacolumn === "siteId") {
			updateData({
				...stateData,
				[datacolumn]: v,
				siteName: v == 0 ? allSiteDisplayText : props.siteList[v].siteName
			});
			setlangDatas( props.siteList[v == 0?1:v].iLangList);
		} else {
			updateData({
				...stateData,
				[datacolumn]: v
			});
		}
	};

	const SetAnnouncement = () => {
		let msg = "";
		let check = true;
		if (!stateData.startTime) {
			check = false;
			msg = "Please select start time.";
		} else if (!stateData.endTime) {
			check = false;
			msg = "Please select end time.";
		} else if (!stateData.subject || !stateData.subject.en) {
			check = false;
			msg = "Please input en subject.";
		} else if (!stateData.content || !stateData.content.en) {
			check = false;
			msg = "Please input en content.";
		}

		if (check) {
			props.SetAnnouncement(stateData, props.IsAdd);
		} else {
			alert(msg);
		}
	};
	

	let langPanels = ( langDatas != null ? langDatas.map(lang => {
		return (
			<OuterPanel
				key={`set-panel-${lang}`}
				HeaderContent={<span style={{ color: "white" }}>{lang}</span>}
				Content={
					<SettingContent
						subject={stateData.subject[lang]}
						content={stateData.content[lang]}
						lang={lang}
						EditFiled={EditFiled}
					/>
				}
			/>
		);
	}):null);
	

	return (
		<>
			<button
				className="btn btn-back"
				style={{ marginBottom: "20px" }}
				onClick={props.ClickBack}>
				Back
			</button>

			<DateTimePickerElem
				columnTitle="From Date"
				dateTimePickerProps={{
					defaultValue: moment().format("l"),
					timeFormat: false,
					onBlur: (e) => {
					try {
						props.onChange(moment(e).format("l"), "ActiveDate");
					} catch (ex) {}
					},
				}}
				/>
			{props.CategoryList !== null ? (
			<DropDownBox
				columnTitle="Category"
				options={props.CategoryList.map((x, index) => {
				return { value: x.categoryId, displayText: x.categoryName };
				})}
				hasAll={false}
				selectedValue={1}
				dataColumn="CategoryId"
				onChange={props.onChange}
			/>
			) : null}
			<InputBox
				Title="User Name"
				onChange={props.onChange}
				dataColumn="UserName"
			/>
			
			<InputBox
				Title="ExtraPoint"
				onChange={props.onChange}
				dataColumn="ExtraPoint"
				type="number"
			/>
			{
				langDatas !=null?
        		langPanels
				:null
			}

			<button
				className="btn btn-add"
				style={{
					marginTop: "20px",
					marginBottom: "40px",
					float: "right"
				}}
				onClick={props.Add_ExtraPointItem}>
				Submit
			</button>
		</>
	);
};

const SwitchBtn = props => {
	return (
		<div className="form-group">
			<label>{props.columnTitle}</label>
			<input
				id={props.Key}
				className="switch switch--flat"
				type="checkbox"
				checked={props.checked}
				onChange={props.EditFiled}
			/>
			<label htmlFor={props.Key}></label>
		</div>
	);
};

const DateTimePickerElem = props => {
	return (
		<div className="form-group">
			<span className="label">{props.columnTitle}</span>
			<DateTimePicker {...props.dateTimePickerProps} />
		</div>
	);
};

const OuterPanel = props => {
	return (
		<div className="panel level-1 mt-20">
			<a
				style={{ justifyContent: "space-between" }}
				className="panel-title">
				{props.HeaderContent}
			</a>
			<div className="panel-body">
				<div className="collapse show">{props.Content}</div>
			</div>
		</div>
	);
};

const SettingContent = props => {
	return (
		<div className="panel-body">
			<div className="form-group">
				<label>Subject</label>
				<input
					type="text"
					className="form-control"
					defaultValue={props.subject}
					onBlur={e => {
						props.EditFiled(e.target.value, "subject", {
							lang: props.lang
						});
					}}
				/>
			</div>
			<div className="form-group">
				<label>Content</label>
				<textarea
					className="form-control"
					rows="8"
					defaultValue={props.content}
					onBlur={e => {
						props.EditFiled(e.target.value, "content", {
							lang: props.lang
						});
					}}></textarea>
			</div>
		</div>
	);
};
const InputBox = (props) => {
    const OnChange = (e) => {
		console.log("e.target",e.target,"props",props);
	  if (props.onChange) {
		props.onChange(e.target.value, props.dataColumn);
	  }
	};
    return (
        <div className={props.ClassName ? props.ClassName : "input-item"}>
            {
                props.HideTitle === true ? null : <label className="label">{props.Title} {props.TitleDetail ? <a href={props.TitleDetail} target="_blank"> (Detail) </a> : null}</label>
            }
            <div className={`field${props.iptSubClass ? (' ' + props.iptSubClass) : ''}`}>
                {
                    props.SubTitle ? <span className="bule-bold">{props.SubTitle}</span> : null
                }
                <input
                    type={props.type ? props.type : 'text'}
                    onChange={OnChange}
                    disabled={props.Disabled === true}
                    className={props.inputClass ? props.inputClass : ''} />
            </div>
        </div>
    )
}
const InputField = (props) => {
	const onChange = (e) => {
		console.log("e.target",e.target,"props",props);
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
export default EditPanel;
