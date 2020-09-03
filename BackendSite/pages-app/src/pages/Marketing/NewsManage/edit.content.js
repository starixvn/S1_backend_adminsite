import React from "react";
import InputBox from "./Component/InputBox";
import TextInputBox from "./Component/TextInputBox";
import DateTimePickerElem from "./Component/DateTimePicker";
import moment from "moment";

export default ({ state, EditValueByLang, EditValue }) => {
	let langPanels = state.langList?.map(l => {
		return (
			<LangPanel
				key={`news-${l}`}
				state={state}
				EditValueByLang={EditValueByLang}
				EditValue={EditValue}
				langId={l}
			/>
		);
	});

	return (
		<>
			<div className="filter noBGcolor"></div>
			{langPanels && langPanels.length > 0 ? (
				<>
					<div className="filter">
						<SwitchBtn
							checked={state.status == 1}
							EditFiled={EditValue.bind(
								this,
								state.status ? 0 : 1,
								"status"
							)}
							columnTitle="Status"
							Key={"checkbox-status"}
						/>
						<DateTimePickerElem
							columnTitle="PostTime"
							dateTimePickerProps={{
								defaultValue: moment(state.postTime).format(
									"l LTS"
								),
								onBlur: e => {
									try {
										EditValue(
											moment(e).format("l LTS"),
											"postTime"
										);
									} catch (ex) {}
								}
							}}
						/>
						<InputBox
							Title="Sort"
							ClassName="form-group"
							inputClass="form-control"
							OnChange={EditValue}
							dataColumn="sort"
							value={state.sort}
						/>
					</div>
					<div className="filter">{langPanels}</div>
				</>
			) : null}
		</>
	);
};

const LangPanel = ({ state, EditValueByLang, langId }) => {
	return (
		<div className="panel level-1 mt-20">
			<a
				className="panel-title no-arrow"
				style={{ justifyContent: "space-between" }}>
				<span style={{ color: "white" }}>{langId}</span>
			</a>
			<div className="panel-body">
				<InputBox
					Title="ImageURL"
					ClassName="form-group"
					inputClass="form-control"
					OnChange={EditValueByLang.bind(this, langId)}
					dataColumn="imageURL"
					value={state.imageURL[langId]}
				/>
				<InputBox
					Title="Title"
					ClassName="form-group"
					inputClass="form-control"
					OnChange={EditValueByLang.bind(this, langId)}
					dataColumn="title"
					value={state.title[langId]}
				/>

				<TextInputBox
					Title="Content"
					ClassName="form-group"
					inputClass="form-control"
					OnChange={EditValueByLang.bind(this, langId)}
					dataColumn="content"
					value={state.content[langId]}
				/>
			</div>
		</div>
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
