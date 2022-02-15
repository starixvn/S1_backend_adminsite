import React, { useState } from "react";
import DateTimePicker from "react-datetime";
import moment from "moment-timezone";

const allSiteDisplayText = "All";

const elemType = {
	SwitchBtn: "SwitchBtn",
	DateTimePicker: "DateTimePicker",
	SeqText:"SeqText"
};

const commonColDefs = [
	{
		datacolumn: "startTime",
		columnTitle: "StartTime",
		elemType: elemType.DateTimePicker
	},
	{
		datacolumn: "endTime",
		columnTitle: "EndTime",
		elemType: elemType.DateTimePicker
	},
	{
		datacolumn: "isTest",
		columnTitle: "IsTest",
		elemType: elemType.SwitchBtn
	},
	{
		datacolumn: "status",
		columnTitle: "Status",
		elemType: elemType.SwitchBtn
	},
	{
		datacolumn: "seq",
		columnTitle: "Display Sequence",
		elemType: elemType.SeqText
	}
];

const EditPanel = props => {
	const [stateData, updateData] = useState({
		...props.Datas,
		subject: props.IsAdd ? {} : JSON.parse(props.Datas.subject),
		content: props.IsAdd ? {} : JSON.parse(props.Datas.content)
	});
	const tempiLangList = [];
	Object.keys(props.siteList).forEach(key=>{
		props.siteList[key].iLangList.forEach(y=>{
			if(!tempiLangList.includes(y)){
				tempiLangList.push(y);
			}
		})
	})
	const [langDatas, setlangDatas] = useState(props.IsAdd ? null : props.Datas.siteId == 0 ? tempiLangList : props.siteList[props.Datas.siteId].iLangList);
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
			const tempAllLang = [];

			Object.keys(props.siteList).forEach(key=>{
				props.siteList[key].iLangList.forEach(y=>{
					if(!tempAllLang.includes(y)){
						tempAllLang.push(y);
					}
				})
			})
			if(v == 0){
				setlangDatas(tempAllLang)
			}
			else{
				setlangDatas(props.siteList[v].iLangList);
			}
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
		}else if (!stateData.seq) {
			check = false;
			msg = "Please input sequence.";
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
	let commonRows = commonColDefs.map((col, cIdx) => {
		let elem = <></>;
		let key = `common-col-${cIdx}`;
		if (col.elemType === elemType.DateTimePicker) {
			elem = (
				<DateTimePickerElem
					key={key}
					columnTitle={col.columnTitle}
					dateTimePickerProps={{
						defaultValue: props.IsAdd
							? ""
							: moment(stateData[col.datacolumn]).format("l LTS"),
						onBlur: e => {
							try {
								EditFiled(
									moment(e).format("l LTS"),
									col.datacolumn
								);
							} catch (ex) {}
						}
					}}
				/>
			);
		} else if (col.elemType === elemType.SwitchBtn) {
			elem = (
				<SwitchBtn
					checked={stateData[col.datacolumn] == 1}
					EditFiled={EditFiled.bind(
						null,
						stateData[col.datacolumn] ? 0 : 1,
						col.datacolumn
					)}
					columnTitle={col.columnTitle}
					Key={
						props.IsAdd
							? `s-c-new-${col.datacolumn}`
							: `s-c-${props.Datas.id}-${col.datacolumn}`
					}
					key={key}
				/>
			);
		}else if (col.elemType === elemType.SeqText){
			elem = (
				<SeqText
					EditFiled={EditFiled}
					columnTitle={col.columnTitle}
					datacolumn={col.datacolumn}
					seq = {stateData[col.datacolumn]}
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
			{commonRows}
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
				onClick={SetAnnouncement}>
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

const SeqText = props => {
	return (
		<div className="form-group">
			<label>{props.columnTitle}</label>
			<input
				className="form-control"
				type="text"
				defaultValue={props.seq}
				onBlur={e => {
					props.EditFiled(e.target.value, props.datacolumn);
				}}
			/>
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

const SiteList = ({ siteList, onChange, currentSelect, datacolumn }) => {
	let radio =
		siteList &&
		Object.keys(siteList).map(siteId => {
			let siteName = siteList[siteId].siteName;
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
			<RadioItem
				value={0}
				dispalyText={allSiteDisplayText}
				id={`rb-site-0`}
				onChange={onChange.bind(null, 0, datacolumn)}
				checked={currentSelect === 0}
			/>
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

export default EditPanel;
